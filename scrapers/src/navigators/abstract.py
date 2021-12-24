import logging, random, time, json
from abc import ABC, abstractmethod

import browsermobproxy
import undetected_chromedriver.v2 as uc

from navigators.kill_handle import KillHandle

class EndOfPage(Exception):
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
	SLOW_MODE_MULTIPLIER = 3.0

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
	# METHODS
	############################################################################	

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

	@staticmethod
	def select_navigator(name: str) -> type:
		import navigators.tiktok, navigators.youtube

		navigator_classes = {x.__name__: x for x in AbstractNavigator.__subclasses__()}
		try:
			return navigator_classes[name]
		except KeyError:
			raise

if __name__ == "__main__":
	AbstractNavigator.select_navigator()