import random
import time
import json
import os
import datetime
import traceback
import re
from abc import abstractmethod
from typing import Dict, List, Optional, Type, TypeVar, Any
from urllib.parse import urlencode

from selenium.webdriver.common import by, action_chains, keys
from selenium.webdriver.support.ui import WebDriverWait
from selenium.common.exceptions import JavascriptException, InvalidSelectorException, \
	ElementNotInteractableException, StaleElementReferenceException, \
	MoveTargetOutOfBoundsException, NoSuchWindowException, TimeoutException
from selenium.webdriver.remote.webelement import WebElement

from db import DBException
from scraper_core import BadArguments, JSException, ScraperCore, ScraperException
from logger import log
from defaults import abstract_navigator as abstract_defaults
from models.options import Options
from navigators.helpers.xpath import XPath
from navigators.helpers.try_to_interact import try_to_interact
from tools import KillHandleTriggered

T = TypeVar("T")

class ScraperMiddleWare(ScraperCore):
	"""
	This class defines a middleware between the specific navigator implementation
	and the Driver-Proxy core.
	"""
	needs_authentication: Optional[bool] = None
	navigator_default_options: Optional[Dict[str, Any]] = None

	# Change to true in derived classes to allow running the program passing
	# the scraper name as the only argument
	allow_empty_options: bool = False

	def __init__(self, options: Options):
		super().__init__(options)
		self.action = None
		try:
			self.validate_options()
			self.test_authentication_options()
		except BadArguments:
			log.critical("An error occurred initializing navigator.")
			self.cleanup(1)

	def validate_options(self):
		"""Derived classes must implement this to check if all necessary options
		were supplied.
		"""
		if self.options.timeout < 0:
			log.critical("timeout must be a non-negative integer")
			raise BadArguments

	def test_authentication_options(self):
		"""This is not a validation, more like a warning.
		"""
		if self.needs_authentication is None:
			log.critical("Variable needs_authentication needs to be "
				f"defined for subclass {type(self).__name__}")
			raise ValueError

		if (self.needs_authentication and any(self.options.anonymization_options)):
			log.warning("The website you are scraping requires "
				"authentication. It is not recommended to use anonymization  "
				"options.")
		elif (not self.needs_authentication and not all(self.options.anonymization_options)):
			log.warning("The website you are scraping does not require "
				"authentication. In spite of that, you are not using every "
				"anonymization option available.")

	def start(self):
		super().start()
		self.action = action_chains.ActionChains(self.driver)
		self.reset_har()
		try:
			log.info(f"Will go to main")
			self.main()
		except NoSuchWindowException as err:
			log.critical("Chrome window was closed from an outside agent!")
			log.critical(err)
			traceback.print_exc()
			self.cleanup(1)
		except TimeoutException as e:
			log.critical("Selenium emitted a TimeoutException!")
			log.critical(e)
			log.critical("Cannot continue! Exiting gracefully...")
			self.cleanup(1)
		except (ScraperException, DBException, KillHandleTriggered):
			self.cleanup(1)
		except Exception as err:
			log.critical("An unknown error happened:")
			log.critical(err)
			traceback.print_exc()
			self.cleanup(1)

	############################################################################
	# NON-IMPLEMENTED METHODS
	############################################################################
	@abstractmethod
	def main(self):
		pass

	############################################################################
	# STATIC METHODS
	############################################################################
	@staticmethod
	def get_available_navigators() -> Dict[str, Type]:
		import navigators.tiktok, navigators.youtube, navigators.test_navigator, \
			navigators.profile_faker, navigators.reels
		return {x.__name__: x for x in ScraperMiddleWare.__subclasses__()}

	@staticmethod
	def match_navigator_name(name: str) -> Optional[Type]:
		available_navigators = ScraperMiddleWare.get_available_navigators()
		for nav_name in available_navigators.keys():
			if re.search(rf"(?i)^{nav_name}$", name):
				return available_navigators[nav_name]
		log.debug(f"Name '{name}' matches no known scraper.")
		return None

	############################################################################
	# METHODS FOR LOCATING
	############################################################################
	def find(self, **kwargs) -> List[WebElement]:
		xpath = XPath.xpath(**kwargs)
		log.debug(f"Looking for elements at xpath = {xpath}")

		try:
			return self.driver.find_elements(by.By.XPATH, xpath)
		except InvalidSelectorException:
			log.critical("Bad xpath selector!")
			raise

	def one(self, result: List[T]) -> T:
		"""From a list of results, returns the first result or raises an error"""
		if len(result) != 1:
			log.critical("A wrong number of elements was returned. "
				f"Expected 1, received {len(result)}")
			raise ValueError
		return result[0]

	def find_one(self, **kwargs) -> WebElement:
		elements = self.find(**kwargs)
		return self.one(elements)

	############################################################################
	# METHODS FOR CHECKING DOM STATE / DELAYING ACTION
	############################################################################
	def wait(
			self,
			timeout: float,
			fuzzy: bool = True) -> None:
		"""Selenium has an inbuilt for this, I'm not sure what advantage it
		brings over using time.sleep"""
		if fuzzy:
			r = ((1.0 - abstract_defaults.WAIT_RANDOM_FACTOR) 
					+ 2.0 * abstract_defaults.WAIT_RANDOM_FACTOR * random.random())
		else:
			r = 1.0
		time.sleep(timeout * r)

	def wait_load(self, timeout: float = None, poll_freq: float = None):
		# Waits a bit, to guarantee last action, triggering the change of
		# document.readyState, was processed
		self.wait(2.0)
		
		timeout = timeout or abstract_defaults.WAIT_UNTIL_TIMEOUT
		poll_freq = poll_freq or abstract_defaults.POLL_FREQUENCY

		wait = WebDriverWait(self.driver, timeout, poll_frequency=poll_freq)

		def page_has_loaded(_):
			return self.driver.execute_script("""
				return document.readyState === "complete";
			""")

		wait.until(page_has_loaded)

	def was_end_of_page_reached(self):
		page_height = self.get_page_height()
		scroll = self.run(f""" return window.scrollY """)
		scroll_height = self.run(f""" return document.body.scrollHeight || document.body.offsetHeight """)
		js_code = f"""
			return (({page_height} + window.scrollY) >= {scroll_height});
		"""
		return self.run(js_code)

	def is_in_view(self, elem: WebElement) -> bool:
		window_inner_width = self.get_page_width()
		window_inner_height = self.get_page_height()
		
		try:
			height = elem.rect["height"]
			width = elem.rect["width"]
			top = elem.rect["x"]
			left = elem.rect["y"]
			bottom = top + height
			right = left + width
		except StaleElementReferenceException:
			return False

		return (
			height > 0 and width > 0
			and (
				top > 0 and top < window_inner_height
				or bottom > 0 and bottom < window_inner_height
			)
			and (
				left > 0 and left < window_inner_width
				or right > 0 and right < window_inner_width
			)
		)

	def get_page_height(self):
		page_height = self.run("""
			return window.innerHeight
			  || document.documentElement.clientHeight
			  || document.body.clientHeight;
		""")
		# If JavaScript fails, we will use 60% of window size
		if page_height is None:
			page_height = self.driver.get_window_size()["height"] * 0.60
		return page_height

	def get_page_width(self):
		page_width = self.run("""
			return window.innerWidth
			  || document.documentElement.clientWidth
			  || document.body.clientWidth;
		""")
		# If JavaScript fails, we will use 60% of window size
		if page_width is None:
			page_width = self.driver.get_window_size()["width"] * 0.60
		return page_width

	############################################################################
	# LOADERS
	############################################################################
	"""
	Some websites load content dynamically as some events are triggered.
	Some of the events they listen to is scrolling, waiting for elements to
	be focused, and element mouse-over. These functions provide some dummy
	interaction in order to load the elements that really interest us
	"""
	def press_tab(self, n_times = 500, timeout = 0.05):
		nodes_visited = set()
		for i in range(n_times):
			self.action.send_keys(keys.Keys.TAB)
			self.action.pause(timeout)
			self.action.perform()

			node_focused = self.run("return document.activeElement ;")
			if node_focused in nodes_visited:
				log.debug(f"After {i} TABs, we have visited every focusable node")
				return
			nodes_visited.add(node_focused)

	def visit_any_link(self, timeout: float):
		anchors = self.find(tag="a")
		visible_anchors = [x for x in anchors if self.is_in_view(x)]
		try:
			anchor = random.choice(visible_anchors)
		except IndexError:
			return
		
		self.action.key_down(keys.Keys.CONTROL)
		self.action.move_to_element(anchor)
		self.action.click(anchor)
		try:
			self.action.perform()
			log.debug(f"Visiting {anchor.get_attribute('href')} briefly...")
		except (ElementNotInteractableException, MoveTargetOutOfBoundsException, StaleElementReferenceException):
			log.debug("Element is not interactable or is out of screen.")
			return

		parent = self.driver.current_window_handle
		children = [x for x in self.driver.window_handles if x != parent]
		log.debug(f"We now have {len(children)} tabs open.")

		if tab_was_not_opened := (len(children) == 0):
			log.debug("Ctrl+Click did not open new tab.")
			self.move_aimlessly(timeout, allow_new_windows=False)

		for tab in children:
			log.debug("Switching to tab")
			self.driver.switch_to.window(tab)
			self.wait_load()
			self.move_aimlessly(timeout, allow_new_windows=False)
			log.debug("Closing tab")
			self.driver.close()

		if tab_was_not_opened:
			log.debug("Going back in page visit history")
			self.run("history.back();")
		else:
			log.debug("Going back to parent tab")
			self.driver.switch_to.window(parent)

	def hover(self, elem: WebElement) -> None:
		self.action.move_to_element(elem)
		self.action.perform()

	def move_aimlessly(
				self,
				timeout: float,
				allow_new_windows: bool = True,
				allow_scrolling: bool = True,
				restore_scrolling: bool = False
				) -> None:
		scroll_x = self.run("return window.scrollX;")
		scroll_y = self.run("return window.scrollY;")

		start = datetime.datetime.now()
		while (datetime.datetime.now() - start).total_seconds() < timeout:
			self.kill_handle.check()
			self.move_mouse_lattice(abstract_defaults.MOVE_AROUND_MOVE_MOUSE_TIMES)
			if allow_scrolling:
				self.scroll_random()
			if allow_new_windows and random.random() < abstract_defaults.MOVE_AROUND_VISIT_LINK_PROB:
				self.visit_any_link(timeout / 2)
			if random.random() < abstract_defaults.LONG_PAUSE_PROBABILITY:
				self.long_pause()
			self.short_pause()

		if restore_scrolling:
			self.run(f"window.scrollTo({scroll_x}, {scroll_y})")

	def move_mouse_lattice(self, number_of_moves: int):
		window_inner_width = self.get_page_width()
		window_inner_height = self.get_page_height()
		
		coords = [
				(x, y)
				for x in range(0, window_inner_width, 10)
				for y in range(0, window_inner_height, 10)
		]
		random.shuffle(coords)
		for i in range(number_of_moves):
			coord = coords[i]
			self.action.move_by_offset(*coord)
			self.action.pause(0.10)
			# Go back to (0, 0)
			self.action.move_by_offset(-coord[0], -coord[1])
			self.action.pause(0.10)
			try:
				self.action.perform()
			except MoveTargetOutOfBoundsException:
				log.debug(f"Mouse would move out of screen, breaking.")
				return

	def move_mouse_around_elem(
				self,
				elem: WebElement,
				min_offset: int = 5,
				max_offset: int = 100
				) -> None:
		offset_x = random.randint(min_offset, max_offset)
		offset_y = random.randint(min_offset, max_offset)

		self.action.move_to_element(elem)
		self.action.pause(0.1)
		self.action.move_by_offset(offset_x, offset_y)
		self.action.perform()

	############################################################################
	# METHODS FOR NAVIGATION AND INTERACTION
	############################################################################
	"""Subclasses are encouraged to reuse these, by wrapping with decorators
	if necessary"""

	def go(self, url: str, query_dict: Optional[Dict] = None) -> None:
		query_encoded = ""
		if query_dict is not None:
			query_encoded = "?" + urlencode(query_dict)
		full_url = f"{url}{query_encoded}"
		log.debug(f"Navigating to {full_url}")
		self.driver.get(full_url)

	@try_to_interact
	def click(self, elem: WebElement):
		elem.click()

	def natural_type(self, elem: WebElement, text: str):
		for c in text:
			elem.send_keys(c)
			time.sleep(random.random() * abstract_defaults.SLOW_TYPE_SLEEP_INTERVAL)

	def run(self, js_code: str) -> Any:
		try:
			return self.driver.execute_script(js_code)
		except JavascriptException:
			log.critical("There is an error in your code:")
			log.critical(js_code)
			raise JSException from JavascriptException

	def injection(self, filename: str) -> Any:
		"""Use filename with extension (normally .js)"""
		log.debug(f"Sending JS injection from file '{filename}'")
		os.chdir(os.path.dirname(os.path.realpath(__file__)))
		try:
			with open(f"./navigators/injections/{filename}", "r") as fp:
				js_code = fp.read()
		except (FileNotFoundError, IsADirectoryError) as e:
			log.critical("File does not exist or is a directory.")
			log.critical(e)
			raise
		except PermissionError as e:
			log.critical("You do not have permissions to open file.")
			log.critical(e)
			raise
		return self.run(js_code)

	def short_pause(self):
		pause_length = abstract_defaults.SHORT_PAUSE_LENGTH
		if self.options.slow_mode:
			pause_length *= abstract_defaults.SLOW_MODE_MULTIPLIER
		time.sleep(pause_length + pause_length * random.random())

	def long_pause(self):
		if random.random() < abstract_defaults.LONG_PAUSE_PROBABILITY:
			pause_length = abstract_defaults.LONG_PAUSE_LENGTH
			if self.options.slow_mode:
				pause_length *= abstract_defaults.SLOW_MODE_MULTIPLIER
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
		"upscroll_proportion" means how much of the scrollings should be upscrolls
		"""
		min_amount_of_scrolling = min_amount_of_scrolling or abstract_defaults.MIN_AMOUNT_OF_SCROLLING
		page_height = self.get_page_height()

		amount = random.randint(min_amount_of_scrolling, page_height)
		if random.random() < upscroll_proportion:
			amount *= -1
		
		self.scroll_exact(amount)

	############################################################################
	# MANIPULATION OF PROXY AND DRIVER
	############################################################################
	def reset_har(self):
		self.proxy.new_har(options=abstract_defaults.HAR_OPTIONS)