#!/usr/bin/env python3
import os
from selenium.webdriver.remote.webelement import WebElement
from selenium.webdriver.common.by import By
from dotenv import dotenv_values

from navigators.abstract import AbstractNavigator
from libs.throttling import throttle

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
		breakpoint()

		os.chdir(os.path.dirname(os.path.realpath(__file__)))
		yt_credentials = dotenv_values("../credentials/youtube.env")
		try:
			account = yt_credentials["account"]
			password = yt_credentials["password"]
		except KeyError:
			self.logger.critical("Could not find credentials!")
			raise

		sign_in = self.find_one("sign in")
		self.click(sign_in)
		self.wait_load()

		email_field = self.find("input[type='email']")
		next_button = self.find_text_node("next")
		self.natural_type(email_field, account)
		self.click(next_button)
		self.wait_load()

		password_field = self.find("input[type='password']")
		next_button = self.find_text_node("next")
		self.natural_type(password_field, password)
		self.click(next_button)
		self.wait_load()

		self.driver.get("https://studio.youtube.com")
		self.wait_load()

		content_button = self.find("a#menu-item-1.menu-item-link")
		self.click(content_button)
		self.wait_load()

		analytics_button = self.find("ytcp-icon-button[aria-label='Analytics']")
		self.click(analytics_button)
		self.wait_load()

		see_more_button = self.find_text_node(
				"see more",
				narrow_by_css="yta-key-metric-card"
		)

		breakpoint()
		pass

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