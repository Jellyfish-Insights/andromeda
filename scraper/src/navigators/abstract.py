import logging, random, time, json
from abc import ABC, abstractmethod
from typing import List

from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.common.exceptions import JavascriptException, InvalidSelectorException, \
	ElementNotInteractableException
from selenium.webdriver.remote.webelement import WebElement

import browsermobproxy
import undetected_chromedriver.v2 as uc

from navigators.helpers.xpath import XPath
from libs.kill_handle import KillHandle

class EndOfPage(Exception):
	pass

class ElementNotFound(Exception):
	pass

class AbstractNavigator(ABC):
	"""
	This class defines the blueprint of a navigator, to be attached to Scraper class.
	"""
	############################################################################
	# CONSTANTS FOR COLLECTING DATA
	############################################################################ss

	# I am not sure we need to capture headers, but still haven't tried to
	# remove this option
	HAR_OPTIONS = {
		'captureHeaders': True,
		'captureContent': True
	}
	############################################################################
	# CONSTANTS FOR NAVIGATION
	############################################################################
	SCROLL_AMOUNT = 200
	SHORT_PAUSE_LENGTH = 1.0
	LONG_PAUSE_LENGTH = 5.0
	LONG_PAUSE_PROBABILITY = 0.10
	UPSCROLL_PROBABILITY = 0.20
	SLOW_MODE_MULTIPLIER = 2.0

	WAIT_UNTIL_TIMEOUT = 10.0
	POLL_FREQUENCY = 0.5

	SLOW_TYPE_SLEEP_INTERVAL = 0.15

	NOT_INTERACTABLE_SLEEP = 0.5
	NOT_INTERACTABLE_RETRY = 10

	def __init__(
				self,
				options: dict,
				driver: uc.Chrome,
				proxy: browsermobproxy.client.Client,
				logger: logging.Logger,
				kill_handle: KillHandle
				):
		
		self.options = options
		self.driver = driver
		self.proxy = proxy
		self.logger = logger
		self.kill_handle = kill_handle

	############################################################################
	# NON-IMPLEMENTED METHODS
	############################################################################

	@abstractmethod
	def build_url(self):
		pass

	@abstractmethod
	def action_load(self):
		pass

	@abstractmethod
	def action_interact(self):
		pass

	############################################################################
	# STATIC METHODS
	############################################################################
	@staticmethod
	def select_navigator(name: str) -> type:
		import navigators.tiktok, navigators.youtube

		navigator_classes = {x.__name__: x for x in AbstractNavigator.__subclasses__()}
		try:
			return navigator_classes[name]
		except KeyError:
			raise

	############################################################################
	# DECORATORS
	############################################################################
	def try_to_interact(func):
		def wrap(*args, **kwargs):
			times = AbstractNavigator.NOT_INTERACTABLE_RETRY
			for _ in range(times):
				try:
					return func(*args, **kwargs)
				except ElementNotInteractableException:
					time.sleep(AbstractNavigator.NOT_INTERACTABLE_SLEEP)
			print(f"Tried {times} times but could not interact with element!")
			raise ElementNotInteractableException
		return wrap

	############################################################################
	# METHODS FOR LOCATING
	############################################################################
	"""It is probably undesirable to change or override these"""

	def find(
				self,
				/,
				tag: str = "*",
				text: str = None,
				text_exact: bool = True,
				case_insensitive: bool = True,
				visible: bool = True,
				attributes: dict = None,
				contains_classes: list = None,
				id: str = None) -> List[WebElement]:

		filters = []

		if visible:
			filters.append(XPath.visible())

		self.logger.debug(f"visible {filters=}")

		if attributes is not None:
			filters.append(XPath.attributes(attributes))

		self.logger.debug(f"attributes {filters=}")

		if contains_classes is not None:
			filters.append(XPath.contains_classes(contains_classes))

		self.logger.debug(f"contains_classes {filters=}")

		if id is not None:
			filters.append(XPath.id(id))

		self.logger.debug(f"id {filters=}")

		if text is not None:
			if text_exact:
				filters.append(XPath.text_exact(text, case_insensitive))
			else:
				filters.append(XPath.text_contains(text, case_insensitive))

		self.logger.debug(f"text_match {filters=}")

		if len(filters) == 0:
			use_filters = ""
		else:
			use_filters = f"[{' and '.join(filters)}]"

		xpath = f"/html/body//{tag}{use_filters}"
		self.logger.debug(f"Looking for elements at xpath = {xpath}")

		try:
			return self.driver.find_elements(By.XPATH, xpath)
		except InvalidSelectorException:
			self.logger.critical("Bad xpath selector!")
			raise

	def one(self, result):
		"""From a list of results, returns the first result or raises an error"""
		if len(result) != 1:
			self.logger.critical("A wrong number of elements was returned. "
				f"Expected 1, received {len(result)}")
			raise ValueError
		return result[0]

	def find_one(
				self,
				/,
				tag: str = "*",
				text: str = None,
				text_exact: bool = True,
				case_insensitive: bool = True,
				visible: bool = True,
				attributes: dict = None,
				contains_classes: list = None,
				id: str = None) -> List[WebElement]:
		
		elements = self.find(
			tag=tag,
			text=text,
			text_exact=text_exact,
			case_insensitive=case_insensitive,
			visible=visible,
			attributes=attributes,
			contains_classes=contains_classes,
			id=id)
		
		return self.one(elements)

	############################################################################
	# METHODS FOR CHECKING DOM STATE / DELAYING ACTION
	############################################################################
	"""It is probably undesirable to change or override these"""
	def wait_load(self, timeout: float = None, poll_freq: float = None):
		# Waits a bit, to guarantee last action, triggering the change of
		# document.readyState, was processed
		time.sleep(2.0)
		
		timeout = timeout or self.WAIT_UNTIL_TIMEOUT
		poll_freq = poll_freq or self.POLL_FREQUENCY

		wait = WebDriverWait(self.driver, timeout, poll_frequency=poll_freq)

		def page_has_loaded(_):
			return self.driver.execute_script("""
				return document.readyState === "complete";
			""")

		wait.until(page_has_loaded)

	############################################################################
	# METHODS FOR INTERACTION
	############################################################################
	"""Subclasses are encouraged to reuse these, by wrapping with decorators
	if necessary"""

	@try_to_interact
	def click(self, elem: WebElement):
		elem.click()

	def natural_type(self, elem: WebElement, text: str):
		for c in text:
			elem.send_keys(c)
			time.sleep(random.random() * self.SLOW_TYPE_SLEEP_INTERVAL)

	def run(self, js_code: str):
		try:
			return self.driver.execute_script(js_code)
		except JavascriptException:
			self.logger.critical("There is an error in your code:")
			self.logger.critical(js_code)
			raise

	def short_pause(self, slow_mode: bool = False):
		pause_length = self.SHORT_PAUSE_LENGTH
		if slow_mode:
			pause_length *= self.SLOW_MODE_MULTIPLIER
		time.sleep(pause_length + pause_length * random.random())

	def long_pause(self, slow_mode: bool = False):
		if random.random() < self.LONG_PAUSE_PROBABILITY:
			pause_length = self.LONG_PAUSE_LENGTH
			if slow_mode:
				pause_length *= self.SLOW_MODE_MULTIPLIER
			time.sleep(pause_length + pause_length * random.random())

	def scroll_down(
			self,
			max_times: int = 10,
			slow_mode: bool = False):
		"""
		Navigates down in the page, trying not to go too fast and emulate a human
		usage.
		"""
		page_height = self.driver.execute_script("return window.innerHeight")
		last_height = None
		scrolled_up = False

		for _ in range(max_times):			
			# Scroll a bit
			amount = random.randint(self.SCROLL_AMOUNT, page_height)
			# Sometimes also scroll up, why not?
			if scrolled_up := (random.random() < self.UPSCROLL_PROBABILITY):
				amount *= -1

			options = {
				"top": amount,
				"left": 0,
				"behavior": "smooth"
			}
			self.driver.execute_script(f"window.scrollBy({json.dumps(options)})")

			new_height = self.driver.execute_script("return window.scrollY")
			if last_height and not scrolled_up and new_height == last_height:
				self.logger.info("End of page was reached.")
				raise EndOfPage

			self.short_pause(slow_mode)
			self.long_pause(slow_mode)

			last_height = new_height

if __name__ == "__main__":
	AbstractNavigator.select_navigator()