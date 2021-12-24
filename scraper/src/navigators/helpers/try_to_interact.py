import time
from typing import Callable
from selenium.common.exceptions import ElementNotInteractableException

NOT_INTERACTABLE_SLEEP = 0.5
NOT_INTERACTABLE_RETRY = 10

def try_to_interact(func: Callable):
	def wrap(*args, **kwargs):
		times = NOT_INTERACTABLE_RETRY
		for _ in range(times):
			try:
				return func(*args, **kwargs)
			except ElementNotInteractableException:
				time.sleep(NOT_INTERACTABLE_SLEEP)
		print(f"Tried {times} times but could not interact with element!")
		raise ElementNotInteractableException
	return wrap