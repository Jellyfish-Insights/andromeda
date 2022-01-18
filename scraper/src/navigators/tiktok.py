#!/usr/bin/env python3
import json, re, random
from typing import Any, Dict, List

from defaults import tiktok as tiktok_defaults
from logger import log
from arg_parser import Options
from scraper_core import BadArguments, UnexpectedResponse
from scraper_middleware import ScraperMiddleWare
from models.account_name import AccountName
from models.video_info import VideoInfo
from db import DBException
from tools import throttle

class TikTok(ScraperMiddleWare):
	"""
	There are two sources of video information, both encoded in JSON format.

	The first is already present in the page's first print. It used to have
	id = "__NEXT_DATA__", but it changed suddenly, so now we have a heuristic
	for finding fitting data on first load, wherever it might be

	The second is the response payload of a request whose URL matches the regex
	listed. This is very fragile at the moment, and should be improved in the
	future

	The structure of the data is the same in both sources, and contains number
	of likes/diggs, views, shares, creation time of the video, and more
	"""
	needs_authentication = False
	navigator_default_options: Dict[str, Any] = tiktok_defaults.NAVIGATOR_DEFAULT_OPTIONS

	GET_REQUEST_URL_RE = re.compile(r'^.+/item_list/\?.*msToken=.*$')
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
			AccountName.test(self.options.account_name)
		except ValueError:
			log.critical("Bad format for TikTok account!")
			raise BadArguments from ValueError

		if self.options.scroll_limit is None or self.options.scroll_limit < 0:
			log.critical("scroll_limit must be a non-negative integer")
			raise BadArguments

	def main(self):
		url = self.build_url()
		self.handle_initial_data(url)
		self.scroll_down_handle_more_data()
	
	def build_url(self) -> str:
		account_name = self.options.account_name
		try:
			AccountName.test(account_name)
		except ValueError:
			raise BadArguments

		language = random.choice(tiktok_defaults.LOCALE_OPTIONS)
		url = f"https://www.tiktok.com/{account_name}?lang={language}"

		for arg in tiktok_defaults.QUERY_STRING:
			url += random.choice(arg)

		return url

	def handle_initial_data(self, url: str):
		self.go(url)
		self.kill_handle.check()

		self.move_aimlessly(timeout = 20.0)

		account_name = self.options.account_name
		self.wait_load()
		log.info("First page load was successful.")
		self.move_aimlessly(timeout = 5.0)

		items = self.injection("tiktok.js")
		if len(items) == 0:
			log.warning("We could not retrieve initial data! You might want to check if you were blocked.")
		for it in items:
			try:
				VideoInfo.add(account_name, it)
			except DBException:
				log.critical("Error interacting with database!")
				raise

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
			self.move_aimlessly(timeout = 1.0)
			log.debug(f"Scrolling down")
			self.scroll_random(upscroll_proportion=tiktok_defaults.UPSCROLL_PROPORTION)
			scrolled += 1
			if self.was_end_of_page_reached():
				stop = True
			self.process_har()

	def process_har(self):
		account_name = self.options.account_name
		relevant_entries: List[Dict] = [
			entry
			for entry in self.proxy.har['log']['entries']
			if self.GET_REQUEST_URL_RE.search(entry['request']['url'])
		]
		for entry in relevant_entries:
			try:
				response_text = self.get_response_text(entry)
				response_payload = self.get_payload_from_response_text(response_text)
				items = self.get_items_from_payload(response_payload)
				for it in items:
					try:
						VideoInfo.add(account_name, it)
					except DBException:
						log.critical("Error interacting with database!")
						raise

			except UnexpectedResponse:
				log.warning("Skipping this entry...")
				continue
			
		self.reset_har()

	@staticmethod
	def get_response_text(entry: dict) -> str:
		try:
			http_status: int = entry["response"]["status"]
			if not (200 <= http_status < 300):
				http_status_text: str = entry["response"]["statusText"]
				log.warning(f"Received non-200 status code: '{http_status}' = '{http_status_text}'")
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
		
		return response_payload

	@staticmethod
	def get_items_from_payload(response_payload: dict) -> List[Dict]:
		try:
			return response_payload["itemList"]
		except KeyError as exc:
			log.warning("Bad format for response payload!")
			log.warning(f"{response_payload=}")
			raise UnexpectedResponse from exc

	############################################################################
	# METHODS FOR NAVIGATION AND INTERACTION
	############################################################################

	@throttle(tiktok_defaults.THROTTLE_EXECUTION_TIME, tiktok_defaults.THROTTLE_AT_LEAST)
	def scroll_random(self, *args, **kwargs):
		return super().scroll_random(*args, **kwargs)

	@throttle(tiktok_defaults.THROTTLE_HOVER)
	def hover(self, xpath_str: str) -> None:
		return super().hover(xpath_str)
