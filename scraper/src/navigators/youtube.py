#!/usr/bin/env python3
import os, re
from collections import OrderedDict
from urllib.parse import urlencode

from selenium.webdriver.remote.webelement import WebElement
from selenium.webdriver.common.by import By
from dotenv import dotenv_values

from navigators.abstract import AbstractNavigator, ElementNotFound
from libs.throttling import throttle

################################################################################
# CONSTANTS
################################################################################

# Convert from query string to python tuples:
# sed -nr "s/&(.+)=(.+)/\('\1', '\2'\),/p" <FILE>

ANALYTICS_QUERY_STRING_LIST = [
	('entity_type', 'VIDEO'),
	('entity_id', 'lkGSGsHHE1Q'),
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
ANALYTICS_QUERY_STRING_ENCODED = urlencode(ANALYTICS_QUERY_STRING_DICT)

################################################################################
# CLASS DEFINITION
################################################################################

class YouTube(AbstractNavigator):
	############################################################################
	# CONSTANTS
	############################################################################
	THROTTLE_EXECUTION_TIME = 0.75
	THROTTLE_AT_LEAST = 0.5

	############################################################################
	# METHODS NOT IMPLEMENTED IN ABSTRACT CLASS
	############################################################################
	def build_url(self):
		return f"https://www.youtube.com"

	def action_load(self):
		os.chdir(os.path.dirname(os.path.realpath(__file__)))
		yt_credentials = dotenv_values("../credentials/youtube.env")
		try:
			account = yt_credentials["account"]
			password = yt_credentials["password"]
		except KeyError:
			self.logger.critical("Could not find credentials!")
			raise

		self.wait_load()
		sign_in_buttons = self.find(
				text="sign in",
				text_exact=True,
				case_insensitive=True
		)
		if sign_in_buttons == 0:
			raise ElementNotFound
		else:
			# Click any of the buttons, we don't care
			sign_in = sign_in_buttons[0]
		self.click(sign_in)
		self.wait_load()

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

		password_field = self.find_one(
			tag="input",
			attributes={"type":"password"}
		)
		next_button = self.find_one(
			text="next",
			text_exact=True,
			case_insensitive=True
		)
		self.natural_type(password_field, password)
		self.click(next_button)
		self.wait_load()

		self.driver.get("https://studio.youtube.com")
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
		video_ids = set(regex.search(x)[1] for x in links)
		self.logger.debug(video_ids)

		for video_id in video_ids:
			self.driver.get(f"https://studio.youtube.com/video/{video_id}/analytics/tab-overview/period-default/explore?{ANALYTICS_QUERY_STRING_ENCODED}")
			self.wait_load()

			download_button = self.find_one(attributes={"icon": "icons:file-download"})
			self.click(download_button)

			csv_button = self.find_one(
				text="comma-separated values (.csv)",
				text_exact=True,
				case_insensitive=True
			)
			self.click(csv_button)
			self.wait_load()
		
		breakpoint()

	def action_interact(self):
		pass

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