#!/usr/bin/env python3
import os
from selenium.webdriver.remote.webelement import WebElement
from dotenv import dotenv_values

from navigators.abstract import AbstractNavigator
from libs.throttling import throttle

class YouTube(AbstractNavigator):
	"""
	This currently does not do anything, but can be used as proof of concept.
	To attach a navigator to the scraper, it is as easy as changing a command
	line option
	"""
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
		self.logger.debug(f"{yt_credentials=}")
		try:
			account = yt_credentials["account"]
			password = yt_credentials["password"]
		except KeyError:
			self.logger.critical("Could not find credentials!")
			raise

		sign_in = self.find_text_node("sign in")
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
	def find(
				self,
				query_selector: str,
				timeout: float = None,
				clickable: bool = False
				) -> WebElement:
		return super().find(query_selector, timeout, clickable)

	@throttle(THROTTLE_EXECUTION_TIME, THROTTLE_AT_LEAST)
	def find_text_node(self, text: str, case_sensitive: bool = False) -> WebElement:
		return super().find_text_node(text, case_sensitive)

	@throttle(THROTTLE_EXECUTION_TIME, THROTTLE_AT_LEAST)
	def wait_load(self, timeout: float = None, poll_freq: float = None):
		return super().wait_load(timeout, poll_freq)