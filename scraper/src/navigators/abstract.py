import logging, random, time, json, math, os, datetime
from abc import ABC, abstractmethod
from typing import Dict, List, TypeVar, Any

from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.common.exceptions import JavascriptException, InvalidSelectorException, \
	ElementNotInteractableException, StaleElementReferenceException, \
	MoveTargetOutOfBoundsException
from selenium.webdriver.remote.webelement import WebElement
from selenium.webdriver.common.action_chains import ActionChains
from selenium.webdriver.common.keys import Keys

import browsermobproxy
import undetected_chromedriver.v2 as uc
from models.options import Options

from navigators.helpers.xpath import XPath
from navigators.helpers.try_to_interact import try_to_interact
from libs.kill_handle import KillHandle

################################################################################
# CONSTANTS
################################################################################

T = TypeVar("T")

################################################################################
# CUSTOM EXCEPTIONS
################################################################################

class EndOfPage(Exception):
	pass

class ElementNotFound(Exception):
	pass

class YouProbablyGotBlocked(Exception):
	"""
	We are as careful as possible not to get blocked, but sometimes it happens.
	Workarounds can be accessing through a proxy, waiting until you are unblocked
	or changing your scraping routine.
	"""
	pass

################################################################################
# CLASS DEFINITION
################################################################################

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
	WAIT_RANDOM_FACTOR = 0.15

	MIN_AMOUNT_OF_SCROLLING = 200
	SHORT_PAUSE_LENGTH = 1.0
	LONG_PAUSE_LENGTH = 5.0
	LONG_PAUSE_PROBABILITY = 0.10
	SLOW_MODE_MULTIPLIER = 2.0

	WAIT_UNTIL_TIMEOUT = 10.0
	POLL_FREQUENCY = 0.5

	SLOW_TYPE_SLEEP_INTERVAL = 0.15

	MOVE_AROUND_MOVE_MOUSE_TIMES = 5
	MOVE_AROUND_VISIT_LINK_PROB = 0.01

	# For performing a sequence of actions as a single block (avoids overhead)
	BATCH_ACTION_SIZE = 10

	def __init__(
				self,
				options: Options,
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
		self.action = ActionChains(driver)

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
	def get_available_navigators() -> Dict[str, type]:
		import navigators.tiktok, navigators.youtube, navigators.test_navigator
		return {x.__name__: x for x in AbstractNavigator.__subclasses__()}

	@staticmethod
	def select_navigator(name: str) -> type:
		navigator_classes = AbstractNavigator.get_available_navigators()
		try:
			return navigator_classes[name]
		except KeyError:
			raise

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

	############################################################################
	# METHODS FOR CHECKING DOM STATE / DELAYING ACTION
	############################################################################
	"""It is probably undesirable to change or override these"""
	def wait(
			self,
			timeout: float,
			fuzzy: bool = True) -> None:
		"""Selenium has an inbuilt for this, I'm not sure what advantage it
		brings over using time.sleep"""
		if fuzzy:
			r = (1.0 - self.WAIT_RANDOM_FACTOR) + 2.0 * self.WAIT_RANDOM_FACTOR * random.random()
		else:
			r = 1.0
		time.sleep(timeout * r)

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
		js_code = """
			return ((window.innerHeight + window.scrollY) >= document.body.offsetHeight);
		"""
		return self.run(js_code)

	def is_in_view(self, elem: WebElement) -> bool:
		window_inner_height = self.run("return window.innerHeight;")
		window_inner_width = self.run("return window.innerWidth;")

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
			self.action.send_keys(Keys.TAB)
			self.action.pause(timeout)
			self.action.perform()

			node_focused = self.run("return document.activeElement ;")
			if node_focused in nodes_visited:
				self.logger.debug(f"After {i} TABs, we have visited every focusable node")
				return
			nodes_visited.add(node_focused)

	def visit_any_link(self, timeout: float):
		anchors = self.find(tag="a")
		visible_anchors = [x for x in anchors if self.is_in_view(x)]
		try:
			anchor = random.choice(visible_anchors)
		except IndexError:
			return
		
		self.action.key_down(Keys.CONTROL)
		self.action.move_to_element(anchor)
		self.action.click(anchor)
		try:
			self.action.perform()
			self.logger.debug(f"Visiting {anchor.get_attribute('href')} briefly...")
		except (ElementNotInteractableException, MoveTargetOutOfBoundsException):
			self.logger.debug("Element is not interactable or is out of screen.")
			return

		parent = self.driver.current_window_handle
		children = self.driver.window_handles

		for window in children:
			if window != parent :
				self.driver.switch_to.window(window)
				self.wait_load()
				self.move_aimlessly(timeout, allow_new_windows=False)
				self.driver.close()

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
			self.move_mouse_lattice(self.MOVE_AROUND_MOVE_MOUSE_TIMES)
			if allow_scrolling:
				self.scroll_random()
			if allow_new_windows and random.random() < self.MOVE_AROUND_VISIT_LINK_PROB:
				self.visit_any_link(timeout / 2)
			if random.random() < self.LONG_PAUSE_PROBABILITY:
				self.long_pause()
			self.short_pause()

		if restore_scrolling:
			self.run(f"window.scrollTo({scroll_x}, {scroll_y})")

	def move_mouse_lattice(self, number_of_moves: int):
		window_inner_width = self.run("return window.innerWidth;")
		window_inner_height = self.run("return window.innerHeight;")
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
				self.logger.debug(f"Mouse would move out of screen, breaking.")
				return
			
	def move_mouse_spiral_center_of_screen(self, max_steps = 200, batch=True):
		window_inner_width = self.run("return window.innerWidth;")
		window_inner_height = self.run("return window.innerHeight;")
		
		center_of_screen = (window_inner_width / 2, window_inner_height / 2)
		self.logger.debug(f"{center_of_screen=}")
		self.action.move_by_offset(*center_of_screen)

		# It will take us 24 steps to go full-circle
		DELTA_T = math.pi / 12
		# Every 360 degrees (24 steps), we will be 50 pixels further from center
		SPIRAL_FACTOR = 50 / (2 * math.pi)

		get_coords = lambda t: (math.cos(t) * SPIRAL_FACTOR * t, math.sin(t) * SPIRAL_FACTOR * t)

		t = 0
		initial_point = last_point = get_coords(t * DELTA_T)
		self.action.move_by_offset(*initial_point)

		for t in range(1, max_steps):
			next_point = get_coords(t * DELTA_T)
			delta = (next_point[0] - last_point[0], next_point[1] - last_point[1])
			self.logger.debug(f"Moving mouse by offset of {delta=}")
			self.action.move_by_offset(*delta)
			self.action.pause(0.01)

			if not batch or t % self.BATCH_ACTION_SIZE == 0:
				try:
					self.action.perform()
				except MoveTargetOutOfBoundsException:
					self.logger.debug(f"Mouse would move out of screen, breaking.")
					return

			last_point = next_point

		self.action.perform()

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

	def go(self, url: str) -> None:
		self.logger.debug(f"Navigating to {url}")
		self.driver.get(url)

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

	def injection(self, filename: str) -> Any:
		"""Use filename with extension (normally .js)"""
		self.logger.debug(f"Sending JS injection from file '{filename}'")
		os.chdir(os.path.dirname(os.path.realpath(__file__)))
		try:
			with open(f"./injections/{filename}", "r") as fp:
				js_code = fp.read()
		except (FileNotFoundError, IsADirectoryError) as e:
			self.logger.critical("File does not exist or is a directory.")
			self.logger.critical(e)
			raise
		except PermissionError as e:
			self.logger.critical("You do not have permissions to open file.")
			self.logger.critical(e)
			raise
		return self.run(js_code)

	def short_pause(self):
		pause_length = self.SHORT_PAUSE_LENGTH
		if self.options.slow_mode:
			pause_length *= self.SLOW_MODE_MULTIPLIER
		time.sleep(pause_length + pause_length * random.random())

	def long_pause(self):
		if random.random() < self.LONG_PAUSE_PROBABILITY:
			pause_length = self.LONG_PAUSE_LENGTH
			if self.options.slow_mode:
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
		"upscroll_proportion" means how much of the scrollings should be upscrolls
		"""
		min_amount_of_scrolling = min_amount_of_scrolling or self.MIN_AMOUNT_OF_SCROLLING
		page_height = self.driver.execute_script("return window.innerHeight")

		amount = random.randint(min_amount_of_scrolling, page_height)
		if random.random() < upscroll_proportion:
			amount *= -1
		
		self.scroll_exact(amount)
