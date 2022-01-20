#!/usr/bin/env python3
import copy
import json
import os
import re
import time
from typing import Any, Dict, Optional, Set, Tuple
from selenium.webdriver.remote.webelement import WebElement

from navigators.helpers.password import SymmetricEncryption
import navigators.helpers.csv_processing as csv_processing
from logger import log
from defaults import youtube as youtube_defaults
from models.options import Options
from scraper_core import ElementNotFound, ScraperException, YouProbablyGotBlocked, BadArguments
from scraper_middleware import ScraperMiddleWare
from tools import get_home_dir, throttle, UseDirectory, find_longest_matching_substring

class YouTube(ScraperMiddleWare):
	needs_authentication = True
	navigator_default_options: Dict[str, Any] = youtube_defaults.NAVIGATOR_DEFAULT_OPTIONS
	############################################################################
	# CONSTANTS
	############################################################################
	THROTTLE_EXECUTION_TIME = 0.75
	THROTTLE_AT_LEAST = 0.5
	THROTTLE_GET_DATA_FOR_VIDEO = 30

	############################################################################
	# CONSTRUCTOR & PROPERTIES
	############################################################################
	def __init__(self, options: Options):
		self._channel_id: Optional[str] = None
		super().__init__(options)

	@property
	def channel_id(self):
		return self._channel_id or "NoChannel"

	############################################################################
	# METHODS
	############################################################################
	def validate_options(self):
		super().validate_options()

		password_exists = (self.options.password_encrypted is not None
				or self.options.password_plain is not None)
		
		if (self.options.credentials_file is None
				and (self.options.account_name is None or not password_exists)):
			log.critical("Credentials must be provided to run the scraper.")
			raise BadArguments

		if self.options.password_plain and self.options.password_encrypted:
			log.critical("Conflicting options: both plain AND encrypted password provided.")
			raise BadArguments

	def main(self):
		url = self.build_url()
		self.go(url)
		# Check if user is already logged in
		avatar_img = self.find(tag="img", attributes={"alt": 'Avatar image'})
		if len(avatar_img) > 0 and self.options.force_logout:
			self.logout()
		
		if len(avatar_img) == 0 or self.options.force_logout:
			account, password = self.get_credentials()
			self.sign_in(account, password)
			
		csv_processing.clean_downloads()
		video_ids = self.get_video_ids()
		for video_id in video_ids:
			self.get_data_for_video(video_id)

		log.info("Compiling extracted data to CSV file")
		csv_processing.process_csv_data()

	def build_url(self):
		return f"https://www.youtube.com"

	############################################################################
	# CUSTOM METHODS
	############################################################################
	def logout(self) -> None:
		self.go("https://www.youtube.com/logout")
		self.wait_load()
		self.go(self.build_url())
	
	def get_credentials(self) -> Tuple[str, str]:
		self.kill_handle.check()
		account = None
		password = None
		if self.options.credentials_file:
			log.debug(f"Reading credentials file at '{self.options.credentials_file}'")
			with open(self.options.credentials_file, "r") as fp:
				yt_credentials: Dict = json.load(self.options.credentials_file)
			if type(yt_credentials) != dict:
				raise ValueError("JSON given corresponds to wrong type of object")
			account = yt_credentials.get("account")
			password = yt_credentials.get("password")
		else:
			log.debug("Using credentials as supplied in CLI options.")
			account = self.options.account_name
			if self.options.password_plain:
				password = self.options.password_plain
			else:
				se = SymmetricEncryption()
				try:
					password = se.decrypt(self.options.password_encrypted)
				except ValueError:
					log.critical("Impossible to obtain a valid password!")
					raise BadArguments from ValueError
		
		if account is None or password is None:
			log.critical("Could not find credentials!")
			raise BadArguments
		return account, password

	def sign_in(self, account: str, password: str) -> None:
		self.kill_handle.check()
		self.wait_load()
		self.move_aimlessly(
			timeout=5.0,
			allow_scrolling=False
		)
		sign_in_buttons = self.find(
				text="sign in",
				text_exact=True,
				case_insensitive=True
		)
		if sign_in_buttons == 0:
			raise ElementNotFound
		else:
			# Click any of the buttons
			sign_in = sign_in_buttons[0]
		
		self.click(sign_in)

		self.wait_load()
		self.move_aimlessly(
			timeout=5.0,
			allow_scrolling=False,
			allow_new_windows=False
		)

		use_another_acc = self.find(
			text="use another account",
			text_exact=True,
			case_insensitive=True
		)
		if len(use_another_acc):
			self.click(use_another_acc[0])

		email_field = self.find_one(
			tag="input",
			attributes={"type":"email"}
		)
		next_button = self.find_one(
			text="next",
			text_exact=True,
			case_insensitive=True
		)
		self.natural_type(email_field, account)
		self.click(next_button)

		self.wait_load()
		self.move_aimlessly(
			timeout=5.0,
			allow_scrolling=False,
			allow_new_windows=False
		)
		try:
			password_field = self.find_one(
				tag="input",
				attributes={"type":"password"}
			)
		except ValueError:
			log.critical("Could not find 'password' field. Check if you "
					"are getting the message 'This browser or app may not be secure.' "
					"Unfortunately, there is no simple workaround.")
			raise YouProbablyGotBlocked
		
		next_button = self.find_one(
			text="next",
			text_exact=True,
			case_insensitive=True
		)
		self.natural_type(password_field, password)
		self.click(next_button)

	def get_video_ids(self) -> Set[str]:
		self.kill_handle.check()
		self.wait_load()
		self.move_aimlessly(
			timeout=20.0,
			restore_scrolling=True
		)
		self.go("https://studio.youtube.com")
		self.wait_load()

		if self.options.managed_account is not None:
			self.access_managed_account()

		self._channel_id = self.run("""
			const regex = new RegExp("^.*/channel/([^?]+).*$");
			let match = window.location.href.match(regex);
			return match ? match[1] : null;
		""")

		if self._channel_id:
			log.info(f"Channel ID is '{self._channel_id}'")
		else:
			log.info("Could not obtain channel id from URL")
			if self.options.managed_account is not None:
				log.critical("Cannot proceed without channel ID!")
				raise ElementNotFound("Channel ID could not be extracted from URL")

		content_button = self.find_one(
			tag="a",
			id="menu-item-1",
			contains_classes=["menu-item-link"]
		)
		self.click(content_button)
		self.wait_load()

		log.debug("Pressing tab a bunch of times to load content...")
		self.press_tab()

		# Using regex in CSS:
		# https://stackoverflow.com/questions/8903313/using-regular-expression-in-css
		js_code = """
			let arr = [];
			document.querySelectorAll("a[href*='watch?v='").forEach(el => {
				arr.push(el.getAttribute("href"));
			});
			return arr;
		"""
		links = self.run(js_code)
		log.debug(f"{links=}")
		if len(links) == 0:
			log.critical("Could not find links to videos!")
			raise ElementNotFound

		regex = re.compile(r"v=(.+)$")
		matches = set(regex.search(x) for x in links)
		video_ids = set(x[1] for x in matches if x is not None)
		log.debug(video_ids)
		return video_ids

	def access_managed_account(self):
		log.info("Logged into manager account. Now switching to client "
			f"account dashboard: '{self.options.managed_account}'")

		avatar_button = self.find_one(tag='button',id='avatar-btn')
		self.click(avatar_button)
		self.move_aimlessly(5.0, allow_new_windows=False, allow_scrolling=False)

		switch_account_button = self.find_one(text='switch account')
		self.click(switch_account_button)
		self.move_aimlessly(3.0, allow_new_windows=False, allow_scrolling=False)

		channel_dict: Dict[str, WebElement] = {
			element.get_attribute("innerText"): element
			for element in self.find(id="channel-title")
		}
		if len(channel_dict) == 0:
			log.critical("No channel titles found!")
			raise ElementNotFound

		channel_titles = channel_dict.keys()
		log.debug(f"Channel titles found are {channel_titles}")

		candidate_account: Optional[WebElement] = None
		for title in channel_titles:
			occurrences_in_page = self.find(text=title, case_insensitive=False)
			if len(occurrences_in_page) == 1:
				log.info(f"We believe we have found the desired account '{title}")
				candidate_account = channel_dict[title]
				break

		if candidate_account is None:
			log.warning("This account manages more than one YouTube account. "
				f"We will try to match managed_account, given as '{self.options.managed_account}'")

			try: 
				best_title, _ = find_longest_matching_substring(
					self.options.managed_account,
					channel_titles,
					threshold=5,
					case_sensitive=False)
			except ValueError as exc:
				log.critical("We are not confident enough to guess.")
				raise ElementNotFound from exc
			
			log.info(f"We will guess that you want to access '{best_title}'")
			candidate_account = channel_dict[title]

		self.click(candidate_account)

	@throttle(THROTTLE_GET_DATA_FOR_VIDEO)
	def get_data_for_video(self, video_id: str) -> None:
		self.kill_handle.check()

		# We don't really need to navigate to this page, as the data is directly
		# accessible in the other link, but that is what a normal user would do
		# and we want to simulate what a normal user does
		query_dict = {}
		self.adapt_query_dict_for_managed_account(query_dict)
		self.go(
			f"https://studio.youtube.com/video/{video_id}/analytics"
				"/tab-overview/period-default", 
			query_dict
		)
		self.wait_load()
		self.move_aimlessly(timeout=5.0)

		query_dict = copy.deepcopy(youtube_defaults.ANALYTICS_QUERY_STRING_DICT)
		query_dict["entity_id"] = video_id
		self.adapt_query_dict_for_managed_account(query_dict)

		self.go(
			f"https://studio.youtube.com/video/{video_id}/analytics"
				"/tab-overview/period-default/explore",
			query_dict
		)
		self.wait_load()
		self.move_aimlessly(timeout=5.0)

		download_button = self.find_one(attributes={"icon": "icons:file-download"})
		self.click(download_button)

		self.wait(1.0)

		csv_button = self.find_one(
			text="comma-separated values (.csv)",
			text_exact=True,
			case_insensitive=True
		)
		log.info(f"Downloading CSV file for '{video_id}'")
		self.click(csv_button)
		self.wait_load()
		self.create_metadata_file(video_id)

	def create_metadata_file(self, video_id: str):
		# Wait for download to complete (it's probably < 10 KB)
		self.wait(5)
		home_dir = get_home_dir()
		downloads_dir = os.path.join(home_dir, "Downloads")
		with UseDirectory(downloads_dir, create_if_nonexistent=False):
			files_by_modification_time = sorted(
				filter(
					os.path.isfile,
					os.listdir()
				),
				key=lambda x: os.path.getmtime(x)
			)
			if not files_by_modification_time:
				log.critical("'Downloads' directory is empty! Nothing was ever downloaded!")
				raise ScraperException()
			last_modified_file = files_by_modification_time[-1]
			metadata = {
				"channelId": self.channel_id,
				"videoId": video_id,
				"timeSaved": int(time.time()),

				# Reserved for future use
				"metric": None,
				"filter": None
			}
			metadata_file = last_modified_file.replace("zip", "json")
			with open(metadata_file, "w") as fp:
				log.info(f"Writing metadata to {metadata_file}")
				fp.write(json.dumps(metadata))

	def adapt_query_dict_for_managed_account(self, query_dict: Dict[str, str]):
		if self.options.managed_account is not None:
			query_dict["c"] = self.channel_id

	############################################################################
	# METHODS DECORATED FROM ABSTRACT CLASS
	############################################################################
	@throttle(THROTTLE_EXECUTION_TIME)
	def click(self, elem: WebElement):
		return super().click(elem)

	@throttle(THROTTLE_EXECUTION_TIME, THROTTLE_AT_LEAST)
	def natural_type(self, elem: WebElement, text: str):
		return super().natural_type(elem, text)

	@throttle(THROTTLE_EXECUTION_TIME, THROTTLE_AT_LEAST)
	def wait_load(self, timeout: float = None, poll_freq: float = None):
		return super().wait_load(timeout, poll_freq)