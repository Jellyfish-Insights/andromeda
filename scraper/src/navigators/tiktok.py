#!/usr/bin/env python3
import json, re, random
from os import kill
from typing import Any, Dict
from selenium.webdriver.common.by import By

from arg_parser import Options
from libs.kill_handle import KillHandleTriggered
from navigators.abstract import AbstractNavigator, EndOfPage
from models.account_name import AccountName
from models.video_info import VideoInfo
from db import DBError
from libs.throttling import throttle

class TikTok(AbstractNavigator):
	"""
	There are two sources of video information, both encoded in JSON format.

	The first is already present in the page's first print, under the script
	tag with id = "__NEXT_DATA__"

	The second is the response payload of a request whose URL matches the regex
	listed

	The structure of the data is the same in both sources, and contains number
	of likes/diggs, views, shares, creation time of the video, and more
	"""
	############################################################################
	# CONSTANTS
	############################################################################
	# A large number of scrolls will be broken into a smaller number so we can
	# parse data as we go
	PARTIAL_SCROLL_SIZE = 10

	LOCALE_OPTIONS = [
		'ar',
		'cs-CZ',
		'de-DE',
		'el-GR',
		'en',
		'es',
		'fr',
		'he-IL',
		'ja-JP',
		'it-IT',
		'pt-BR',
		'ru-RU',
		'tr-TR',
		'zh-Hant-TW'
	]

	QUERY_STRING = [
		('&is_copy_url=1',''),
		('&is_from_webapp=v1','')
	]

	THROTTLE_EXECUTION_TIME = 2.00
	THROTTLE_AT_LEAST = 2.00

	THROTTLE_HOVER = 0.25

	UPSCROLL_PROPORTION = 0.20

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
		
		if options.account_name is None:
			logger.critical("Account name must be provided to run the scraper.")
			raise AttributeError
		
		try:
			AccountName.test(options.account_name)
		except ValueError:
			logger.critical("Bad format for TikTok account!")
			raise
		
		super().__init__(options, driver, proxy, logger, kill_handle)

	############################################################################
	# METHODS
	############################################################################
	def build_url(self):
		account_name = self.options.account_name

		try:
			AccountName.test(account_name)
		except ValueError:
			raise

		language = random.choice(self.LOCALE_OPTIONS)
		url = f"https://www.tiktok.com/{account_name}?lang={language}"

		for arg in self.QUERY_STRING:
			url += random.choice(arg)

		return url

	def action_load(self):
		try:
			self.kill_handle.check()
		except KillHandleTriggered:
			raise

		self.move_aimlessly(timeout = 20.0)

		account_name = self.options.account_name
		self.wait_load()
		self.logger.info("First page load was successful.")
		self.move_aimlessly(timeout = 5.0)

		items = self.injection("tiktok.js")
		if len(items) == 0:
			self.logger.warning("We could not retrieve initial data! You might want to check if you were blocked.")
		for it in items:
			try:
				VideoInfo.add(account_name, it)
			except DBError:
				self.logger.critical("Error interacting with database!")
				raise

		return True

	def action_interact(self):
		self.logger.info("Now let's scroll down to get more data... will scroll " +
			f"down up to {self.options.scroll_limit} times")

		stop = False
		scrolled = 0
		while (not stop 
				and (self.options.scroll_limit == 0 
				or scrolled < self.options.scroll_limit)):
			self.kill_handle.check()
			self.move_aimlessly(timeout = 1.0)
			self.logger.debug(f"Scrolling down")
			self.scroll_random(upscroll_proportion=self.UPSCROLL_PROPORTION)
			scrolled += 1
			if self.was_end_of_page_reached():
				stop = True
			self.process_har()

	def process_har(self):
		account_name = self.options.account_name

		for ent in self.proxy.har['log']['entries']:
			url = ent['request']['url']
			if re.search(r'^.+/item_list/\?.*msToken=.*$',url):
				try:
					response_payload = json.loads(ent['response']['content']['text'])
					if type(response_payload) == dict:
						items = response_payload["itemList"]
						for it in items:
							try:
								VideoInfo.add(account_name, it)
							except DBError:
								self.logger.critical("Error interacting with database!")
								raise

				except KeyError:
					self.logger.warning("Could not fetch information from GET request")

		# Reset the HAR
		self.proxy.new_har(options=self.HAR_OPTIONS)


	############################################################################
	# METHODS FOR NAVIGATION AND INTERACTION
	############################################################################

	@throttle(THROTTLE_EXECUTION_TIME, THROTTLE_AT_LEAST)
	def scroll_random(self, *args, **kwargs):
		return super().scroll_random(*args, **kwargs)

	@throttle(THROTTLE_HOVER)
	def hover(self, xpath_str: str) -> None:
		return super().hover(xpath_str)