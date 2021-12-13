#!/usr/bin/env python3
import json, re, random
from os import kill
from selenium.webdriver.common.by import By

from libs.kill_handle import KillHandleTriggered
from navigators.abstract import AbstractNavigator, EndOfPage
from models.account_name import AccountName
from models.video_info import VideoInfo
from db import DBError
from scraper import LOCALE_OPTIONS

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

	############################################################################
	# CONSTRUCTOR
	############################################################################
	def __init__(self,
				options: dict,
				driver,
				proxy,
				logger,
				kill_handle
				):
		
		if not hasattr(options, "account_name") or options.account_name is None:
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

		account_name = self.options.account_name

		self.logger.info("First page load was successful. Printing initial data...")
		next_data = json.loads(
			self.driver.find_element(
				By.ID, "__NEXT_DATA__").get_attribute('innerHTML'))
		try:
			items = next_data["props"]["pageProps"]["items"]

			for it in items:
				try:
					VideoInfo.add(account_name, it)
				except DBError:
					self.logger.critical("Error interacting with database!")
					raise

		except KeyError:
			self.logger.warning("Could not fetch information from __NEXT_DATA__")

		return True

	def action_interact(self):
		account_name = self.options.account_name

		self.logger.info("Now let's scroll down to get more data... will scroll " +
			f"down up to {self.options.scroll_limit} times")

		scrolled = 0
		while scrolled < self.options.scroll_limit:
			self.kill_handle.check()

			will_scroll = min(self.options.scroll_limit - scrolled, self.PARTIAL_SCROLL_SIZE)
			self.logger.info(f"We will now scroll a bit ({will_scroll} times)")
			try:
				self.scroll_down(
					max_times = will_scroll,
					slow_mode = self.options.slow_mode
				)
				scrolled += will_scroll
			except EndOfPage:
				scrolled = self.options.scroll_limit

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
