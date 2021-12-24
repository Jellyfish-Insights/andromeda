#!/usr/bin/env python3
import os
from selenium.webdriver.remote.webelement import WebElement
from selenium.webdriver.common.by import By
from dotenv import dotenv_values

from navigators.abstract import AbstractNavigator, ElementNotFound
from libs.throttling import throttle
from navigators.helpers.xpath import XPath

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

		items_to_hover = self.find(
			tag="div",
			id="hover-items"
		)
		for i in range(len(items_to_hover)):
			xpath = XPath.xpath(
				tag = "div",
				id="hover-items",
				n_th=i
			)
			self.hover(xpath, max_elem_to_hover=1)
			self.wait(0.5)
			analytics_button = self.find_one(
				tag="ytcp-icon-button",
				attributes={"aria-label":'Analytics'}
			)
			self.click(analytics_button)
			self.wait_load()
			see_more_button = self.find_one(
					tag="yta-key-metric-card",
					text="see more",
					text_exact=True,
					case_insensitive=True
			)
			self.click(see_more_button)
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