import logging, random, time, json
from abc import ABC, abstractmethod
from typing import Iterator, List, TypeVar, Any

from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.common.exceptions import JavascriptException, InvalidSelectorException, \
	ElementNotInteractableException
from selenium.webdriver.remote.webelement import WebElement

import browsermobproxy
import undetected_chromedriver.v2 as uc

from navigators.helpers.xpath import XPath
from navigators.helpers.trim import trim
from libs.kill_handle import KillHandle

T = TypeVar("T")

class EndOfPage(Exception):
	pass

class ElementNotFound(Exception):
	pass

class WrongNumberOfElements(Exception):
	"""
	Some of our functions can only take an xpath returning exactly ONE
	page element. If more than that is returned, or if zero elements are
	returned, we issue an exception
	"""
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
	MIN_AMOUNT_OF_SCROLLING = 200
	SHORT_PAUSE_LENGTH = 1.0
	LONG_PAUSE_LENGTH = 5.0
	LONG_PAUSE_PROBABILITY = 0.10
	SLOW_MODE_MULTIPLIER = 2.0

	WAIT_UNTIL_TIMEOUT = 10.0
	POLL_FREQUENCY = 0.5

	SLOW_TYPE_SLEEP_INTERVAL = 0.15

	NOT_INTERACTABLE_SLEEP = 0.5
	NOT_INTERACTABLE_RETRY = 10

	MOVE_AROUND_MAX_HOVERS = 10
	MOVE_AROUND_PROB_SCROLL_ON_HOVER = 0.25

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
	# DECORATORS / HIGHER ORDER FUNCTIONS
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

	def find(self, **kwargs) -> List[WebElement]:
		xpath = XPath.xpath(**kwargs)
		self.logger.debug(f"Looking for elements at xpath = {xpath}")

		try:
			return self.driver.find_elements(By.XPATH, xpath)
		except InvalidSelectorException:
			self.logger.critical("Bad xpath selector!")
			raise

	def one(self, result: List[T]) -> T:
		"""From a list of results, returns the first result or raises an error"""
		if len(result) != 1:
			self.logger.critical("A wrong number of elements was returned. "
				f"Expected 1, received {len(result)}")
			raise ValueError
		return result[0]

	def find_one(self, **kwargs) -> WebElement:
		elements = self.find(**kwargs)
		return self.one(elements)

	def xpath_len(self, xpath_str: str) -> int:
		"""
		Returns how many elements are pointed by the given xpath string.
		"""
		elements = self.driver.find_elements(By.XPATH, xpath_str)
		return len(elements)

	def xpath_one(self, xpath_str: str) -> bool:
		"""
		Does the given xpath point to exactly one element?
		"""
		return self.xpath_len(xpath_str) == 1

	def get_xpath_iterator(
				self,
				xpath_str: str,
				expected: int = None,
				random_order: bool = False
				) -> Iterator:
		elements = self.driver.find_elements(By.XPATH, xpath_str)
		len_elements = len(elements)

		if expected is not None and expected != len_elements:
			self.logger.warning(f"You were expecting to iterate over {expected} elements, "
				f"but selector returned {len_elements} elements.")

		indices = list(range(len_elements))
		if random_order:
			random.shuffle(indices)

		for i in indices:
			yield XPath.nth(xpath_str, i)

	############################################################################
	# METHODS FOR CHECKING DOM STATE / DELAYING ACTION
	############################################################################
	"""It is probably undesirable to change or override these"""
	def wait(self, timeout: float):
		"""Selenium has an inbuilt for this, I'm not sure what advantage it
		brings over using time.sleep"""
		time.sleep(timeout)

	def wait_load(self, timeout: float = None, poll_freq: float = None):
		# Waits a bit, to guarantee last action, triggering the change of
		# document.readyState, was processed
		self.wait(2.0)
		
		timeout = timeout or self.WAIT_UNTIL_TIMEOUT
		poll_freq = poll_freq or self.POLL_FREQUENCY

		wait = WebDriverWait(self.driver, timeout, poll_frequency=poll_freq)

		def page_has_loaded(_):
			return self.driver.execute_script("""
				return document.readyState === "complete";
			""")

		wait.until(page_has_loaded)

	def was_end_of_page_reached(self):
		# Scrolling by 1 doesn't work well, so we are using a larger value
		js_code = """
			return ((window.innerHeight + window.scrollY) >= document.body.offsetHeight);
		"""
		return self.run(js_code)

	def is_in_view(self, xpath_str: str) -> bool:
		"""
		Only one element should be returned by xpath_str, please use appropriate
		filter in XPath if more than one result is returned
		"""

		if not self.xpath_one(xpath_str):
			self.logger.critical(f"xpath returned {self.xpath_len(xpath_str)} "
					"results, expected one.")
			self.logger.critical(f"xpath_str is <<< {xpath_str} >>>")
			raise WrongNumberOfElements

		js_code = f"""
		window.isInView = function(elem)
		{{
			const rect = elem.getBoundingClientRect();
			const innerHeight = window.innerHeight;
			const innerWidth = window.innerWidth;
			return (
				rect.height > 0 && rect.width > 0 
				&& (
					rect.top > 0 && rect.top < innerHeight
					|| rect.bottom > 0 && rect.bottom < innerHeight
				)
				&& (
					rect.left > 0 && rect.left < innerWidth
					|| rect.right > 0 && rect.right < innerWidth
				)
			);
		}}

		const elem = {XPath.get_one_element_js(xpath_str)} ;
		return isInView(elem);
		"""
		return self.run(js_code)

	############################################################################
	# METHODS FOR NAVIGATION AND INTERACTION
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

	def run(self, js_code: str) -> Any:
		try:
			return self.driver.execute_script(js_code)
		except JavascriptException:
			self.logger.critical("There is an error in your code:")
			self.logger.critical(js_code)
			raise

	def trim_and_run(self, js_code: str) -> Any:
		"""
		ChromeDriver seems to have no issue with receiving code with a lot of
		blank space.
		
		But in case you want to trim code for debugging purposes, here's an
		option for that.
		"""
		trimmed_code = trim(js_code)
		self.logger.debug(f"Code before trimming <<< {js_code} >>>")
		self.logger.debug(f"Code after trimming <<< {trimmed_code} >>>")
		return self.run(trimmed_code)

	def hover(self, xpath_str: str) -> None:
		"""
		Only one element should be returned by xpath_str, please use appropriate
		filter in XPath if more than one result is returned
		"""

		if not self.xpath_one(xpath_str):
			self.logger.critical(f"xpath returned {self.xpath_len(xpath_str)} "
					"results, expected one.")
			self.logger.critical(f"xpath_str is <<< {xpath_str} >>>")
			raise WrongNumberOfElements

		js_code = f"""
		const hoverEvent = new MouseEvent('mouseover', {{
			'view': window,
			'bubbles': true,
			'cancelable': true
		}});

		const elem = {XPath.get_one_element_js(xpath_str)} ;
		elem.dispatchEvent(hoverEvent);
		"""
		self.run(js_code)

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

	def scroll_exact(self, amount: int):
		"""
		A negative value for "amount" will mean scrolling up, a positive
		for scrolling down.
		"""
		options = {
			"top": amount,
			"left": 0,
			"behavior": "smooth"
		}
		self.driver.execute_script(f"window.scrollBy({json.dumps(options)})")

	def scroll_random(
			self,
			min_amount_of_scrolling: int = None,
			upscroll_proportion: float = 0.50):
		"""
		Navigates down in the page, trying not to go too fast and emulate a human
		usage.
		"upscroll_proportion" means how much of the scrollings should be upscrolls
		"""
		min_amount_of_scrolling = min_amount_of_scrolling or self.MIN_AMOUNT_OF_SCROLLING
		page_height = self.driver.execute_script("return window.innerHeight")

		amount = random.randint(min_amount_of_scrolling, page_height)
		if random.random() < upscroll_proportion:
			amount *= -1
		
		self.scroll_exact(amount)

	def move_around(
				self,
				max_hovers = None,
				prob_of_scroll_on_hover = None
				) -> None:
		"""
		This will move around the page aimlessly. The purpose is to look like
		a normal user and get unloaded elements to load.

		Please use throttlers in the concrete implementation, or this could go
		too fast and look too robotic.
		"""
		max_hovers = max_hovers or self.MOVE_AROUND_MAX_HOVERS
		prob_of_scroll_on_hover = prob_of_scroll_on_hover or self.MOVE_AROUND_PROB_SCROLL_ON_HOVER
		self.scroll_random()
		
		xpath_list = XPath.xpath()
		hovered = 0
		for elem in self.get_xpath_iterator(xpath_list, random_order=True):
			if self.is_in_view(elem):
				self.hover(elem)
				hovered += 1
				if random.random() < prob_of_scroll_on_hover:
					self.scroll_random()
				if hovered >= max_hovers:
					break

if __name__ == "__main__":
	AbstractNavigator.select_navigator()