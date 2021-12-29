#!/usr/bin/env python3
import re
from collections import OrderedDict
from typing import Set, Tuple
from urllib.parse import urlencode

from selenium.webdriver.remote.webelement import WebElement
from dotenv import dotenv_values

from arg_parser import Options
from navigators.abstract import AbstractNavigator, ElementNotFound, YouProbablyGotBlocked
from libs.throttling import throttle

################################################################################
# CONSTANTS
################################################################################

# Convert from query string to python tuples:
# sed -nr "s/&(.+)=(.+)/\('\1', '\2'\),/p" <FILE>

ANALYTICS_QUERY_STRING_LIST = [
	('entity_type', 'VIDEO'),
	('entity_id', '<USE_YOUR_VIDEO_ID_HERE>'),
	# You won't be able to export to CSV if you choose "since_publish"
	('time_period', '4_weeks'),
	('explore_type', 'TABLE_AND_CHART'),
	('metric', 'VIEWS'),
	('granularity', 'DAY'),
	('t_metrics', 'VIEWS'),
	('t_metrics', 'WATCH_TIME'),
	('t_metrics', 'SUBSCRIBERS_NET_CHANGE'),
	('t_metrics', 'VIDEO_THUMBNAIL_IMPRESSIONS'),
	('t_metrics', 'VIDEO_THUMBNAIL_IMPRESSIONS_VTR'),
	('v_metrics', 'VIEWS'),
	('v_metrics', 'WATCH_TIME'),
	('v_metrics', 'SUBSCRIBERS_NET_CHANGE'),
	('v_metrics', 'VIDEO_THUMBNAIL_IMPRESSIONS'),
	('v_metrics', 'VIDEO_THUMBNAIL_IMPRESSIONS_VTR'),
	('dimension', 'VIDEO'),
	('o_column', 'VIEWS'),
	('o_direction', 'ANALYTICS_ORDER_DIRECTION_DESC'),
]

ANALYTICS_QUERY_STRING_DICT = OrderedDict(ANALYTICS_QUERY_STRING_LIST)

################################################################################
# CLASS DEFINITION
################################################################################

class YouTube(AbstractNavigator):
	############################################################################
	# CONSTANTS
	############################################################################
	THROTTLE_EXECUTION_TIME = 0.75
	THROTTLE_AT_LEAST = 0.5
	THROTTLE_GET_DATA_FOR_VIDEO = 30

	############################################################################
	# CONSTRUCTOR
	############################################################################

	def __init__(self,
				options: Options,
				driver,
				proxy,
				logger,
				kill_handle
				):
		
		if options.credentials_file is None:
			logger.critical("Credentials file must be provided to run the scraper.")
			raise AttributeError
		
		super().__init__(options, driver, proxy, logger, kill_handle)

	############################################################################
	# METHODS
	############################################################################
	def main(self):
		url = self.build_url()
		self.go(url)
		account, password = self.get_credentials()
		self.sign_in(account, password)
		video_ids = self.get_video_ids()
		for video_id in video_ids:
			self.get_data_for_video(video_id)

	def build_url(self):
		return f"https://www.youtube.com"

	############################################################################
	# CUSTOM METHODS
	############################################################################
	def get_credentials(self) -> Tuple[str, str]:
		self.kill_handle.check()
		self.logger.debug(f"Reading credentials file at '{self.options.credentials_file}'")
		yt_credentials = dotenv_values(self.options.credentials_file)
		account = yt_credentials.get("account")
		password = yt_credentials.get("password")
		if account is None or password is None:
			self.logger.critical("Could not find credentials!")
			raise KeyError
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
			self.logger.critical("Could not find 'password' field. Check if you "
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

		content_button = self.find_one(
			tag="a",
			id="menu-item-1",
			contains_classes=["menu-item-link"]
		)
		self.click(content_button)
		self.wait_load()

		self.logger.debug("Pressing tab a bunch of times to load content...")
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
		self.logger.debug(f"{links=}")
		if len(links) == 0:
			self.logger.critical("Could not find links to videos!")
			raise ElementNotFound

		regex = re.compile(r"v=(.+)$")
		matches = set(regex.search(x) for x in links)
		video_ids = set(x[1] for x in matches if x is not None)
		self.logger.debug(video_ids)
		return video_ids

	@throttle(THROTTLE_GET_DATA_FOR_VIDEO)
	def get_data_for_video(self, video_id: str) -> None:
		self.kill_handle.check()

		# We don't really need to navigate to this page, as the data is directly
		# accessible in the other link, but that is what a normal user would do
		# and we want to simulate what a normal user does
		self.go(f"https://studio.youtube.com/video/{video_id}/analytics/tab-overview/period-default")
		self.wait_load()
		self.move_aimlessly(timeout=5.0)

		url_dict = ANALYTICS_QUERY_STRING_DICT
		url_dict["entity_id"] = video_id
		url_encoded = urlencode(ANALYTICS_QUERY_STRING_DICT)

		self.go(f"https://studio.youtube.com/video/{video_id}/analytics/tab-overview/period-default/explore?{url_encoded}")
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
		self.logger.info(f"Downloading CSV file for '{video_id}'")
		self.click(csv_button)
		self.wait_load()

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