#!/usr/bin/env python3
import json, re, random
from typing import Any, Dict, List

from defaults import reels as reels_defaults
from logger import log
from arg_parser import Options
from scraper_core import BadArguments, UnexpectedResponse, YouProbablyGotBlocked
from scraper_middleware import ScraperMiddleWare
from models.account_name import AccountName
from models.video_info import VideoInfo
from db import DBException
from tools import throttle

class Reels(ScraperMiddleWare):

	needs_authentication = True
	navigator_default_options: Dict[str, Any] = reels_defaults.NAVIGATOR_DEFAULT_OPTIONS

	GET_REQUEST_URL_RE = re.compile(r'^.+/api/v./clips/user/$')
	############################################################################
	# CONSTRUCTOR
	############################################################################
	def __init__(self, options: Options):
		super().__init__(options)
		
	############################################################################
	# METHODS
	############################################################################
	def validate_options(self):
		super().validate_options()
		if self.options.account_name is None:
			log.critical("Account name must be provided to run the scraper.")
			raise BadArguments

		try:
			AccountName.test_reels(self.options.account_name)
		except ValueError:
			log.critical("Bad format for Reels account!")
			raise BadArguments from ValueError

		if self.options.scroll_limit is None or self.options.scroll_limit < 0:
			log.critical("scroll_limit must be a non-negative integer")
			raise BadArguments

	def main(self):
		if (self.options.account_name is None or self.options.password_plain is None):
			log.critical("Credentials must be provided to run the scraper.")
			raise BadArguments
		self.go("https://www.instagram.com/")
		# Check if user is already logged in
		instagram_img = self.find(contains_classes={"coreSpriteLoggedOutWordmark"})
		if(len(instagram_img) <= 0):
			self.sign_out()
		self.sign_in(self.options.account_name, self.options.password_plain)
		url = self.build_url()
		self.handle_initial_data(url)
		self.scroll_down_handle_more_data()
	
	def build_url(self) -> str:
		account_name = self.options.managed_account
		try:
			AccountName.test_reels(account_name)
		except ValueError:
			raise BadArguments

		#language = random.choice(reels_defaults.LOCALE_OPTIONS)
		url = f"https://www.instagram.com/{account_name}/reels/"

		for arg in reels_defaults.QUERY_STRING:
			url += random.choice(arg)

		return url

	def handle_initial_data(self, url: str):
		self.go(url)
		self.kill_handle.check()

		self.move_aimlessly(timeout = 20.0, allow_new_windows=False)

		account_name = self.options.account_name
		self.wait_load()
		log.info("First page load was successful.")
		self.move_aimlessly(timeout = 5.0, allow_new_windows=False)
		self.process_har()

	def scroll_down_handle_more_data(self):
		self.kill_handle.check()
		if self.options.scroll_limit != 0:
			scroll_limit_string = f"up to {self.options.scroll_limit} times"
		else:
			scroll_limit_string = "until the end of the page or timeout"
		log.info("Now let's scroll down to get more data... will scroll " +
			f"down {scroll_limit_string}")

		stop = False
		scrolled = 0
		while (not stop 
				and (self.options.scroll_limit == 0 
				or scrolled < self.options.scroll_limit)):
			self.kill_handle.check()
			self.move_aimlessly(timeout = 1.0, allow_new_windows=False)
			log.debug(f"Scrolling down")
			self.scroll_random(min_amount_of_scrolling=400, upscroll_proportion=reels_defaults.UPSCROLL_PROPORTION)
			scrolled += 1
			if self.was_end_of_page_reached():
				stop = True
			self.process_har()

	def process_har(self):
		log.info("Will start processing HAR.")
		"""See HAR specifications here: https://archive.is/Ud8mh
		"""
		account_name = self.options.managed_account
		relevant_entries: List[Dict] = [
			entry
			for entry in self.proxy.har['log']['entries']
			if (
				entry["request"]["method"] == "POST" and
				self.GET_REQUEST_URL_RE.search(entry['request']['url'])
			)
		]
		for entry in relevant_entries:
			try:
				response_text = self.get_response_text(entry)
				response_payload = self.get_payload_from_response_text(response_text)
				items = self.get_items_from_payload(response_payload)
				for it in items:
					try:
						item_type = it["media"]["product_type"]
						if item_type == "clips":
							VideoInfo.add_reels(account_name, it["media"])
						else:
							log.debug(f"Item found was not a reel. Type: {item_type}")
					except DBException:
						log.critical("Error interacting with database!")
						raise

			except UnexpectedResponse:
				log.warning("Skipping this entry...")
				continue

		self.reset_har()

	def sign_in(self, account: str, password: str) -> None:
		self.kill_handle.check()
		self.wait_load()
		self.move_aimlessly(
			timeout=5.0,
			allow_scrolling=False,
			allow_new_windows=False
		)

		user_field = self.find_one(
			tag="input",
			attributes={"type":"text", "name":"username"}
		)
		self.natural_type(user_field, account)

		self.move_aimlessly(
			timeout=5.0,
			allow_scrolling=False,
			allow_new_windows=False
		)
		try:
			password_field = self.find_one(
				tag="input",
				attributes={"type":"password", "name":"password"}
			)
		except ValueError:
			log.critical("Could not find 'password' field. Check if you "
					"are getting the message 'This browser or app may not be secure.' "
					"Unfortunately, there is no simple workaround.")
			raise YouProbablyGotBlocked
		
		next_button = self.find_one(
			text="Log In",
			text_exact=True,
			case_insensitive=False
		)
		self.natural_type(password_field, password)
		self.click(next_button)
		self.wait_load()

	def sign_out(self) -> None:
		self.kill_handle.check()
		self.wait_load()
		self.move_aimlessly(
			timeout=5.0,
			allow_scrolling=False,
			allow_new_windows=False
		)
		avatar_button = self.find_one(
			contains_classes={"_2dbep qNELH"},
			attributes={"role":"link"}
		)
		self.click(avatar_button)

		self.wait_load()
		self.move_aimlessly(
			timeout=5.0,
			allow_scrolling=False,
			allow_new_windows=False
		)
		logout_button = self.find_one(
			text="Log Out",
			text_exact=True,
			case_insensitive=False
		)
		self.click(logout_button)
		self.wait_load()

	@staticmethod
	def get_response_text(entry: dict) -> str:
		try:
			http_status: int = entry["response"]["status"]
			if not (200 <= http_status < 300):
				http_status_text: str = entry["response"]["statusText"]
				log.warning(f"Received non-200 status code: '{http_status}' = '{http_status_text}'")
				raise UnexpectedResponse

			if (size := entry["response"]["content"]["size"]) <= 0:
				log.warning(f"Response content size is non-positive integer: {size} bytes")
				raise UnexpectedResponse

			if (body_size := entry["response"]["bodySize"]) <= 0:
				log.warning(f"Response body size is non-positive integer: {body_size} bytes")
				raise UnexpectedResponse

			return entry['response']['content']['text']

		except KeyError as exc:
			log.warning("Cannot parse response from GET request")
			log.warning(f"{entry=}")
			raise UnexpectedResponse from exc

	@staticmethod
	def get_payload_from_response_text(response_text: str) -> dict:
		try:
			response_payload = json.loads(response_text)
		except json.decoder.JSONDecodeError as exc:
			log.warning("JSON Decode Error!")
			log.warning(exc)
			log.warning("Here's the object trying to be read by json.loads:")
			log.warning(f"{response_text=}")
			raise UnexpectedResponse from exc
		
		if type(response_payload) != dict:
			log.warning("Response payload decoded to something that is not a "
				f"dict! (type = {type(response_payload)})")
			log.warning(f"{response_payload=}")
			raise UnexpectedResponse
		
		return response_payload

	@staticmethod
	def get_items_from_payload(response_payload: dict) -> List[Dict]:
		try:
			return response_payload["items"]
		except KeyError as exc:
			log.warning("Bad format for response payload!")
			log.warning(f"{response_payload=}")
			raise UnexpectedResponse from exc

	############################################################################
	# METHODS FOR NAVIGATION AND INTERACTION
	############################################################################

	@throttle(reels_defaults.THROTTLE_EXECUTION_TIME, reels_defaults.THROTTLE_AT_LEAST)
	def scroll_random(self, *args, **kwargs):
		return super().scroll_random(*args, **kwargs)

	@throttle(reels_defaults.THROTTLE_HOVER)
	def hover(self, xpath_str: str) -> None:
		return super().hover(xpath_str)
