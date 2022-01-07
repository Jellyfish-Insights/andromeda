import threading
import time
import datetime
import random
import os
import re
from typing import Set, Union

from logger import log

def throttle(
			execution_time: float,
			sleep_at_least: float = 0.0,
			random_factor:  float = 0.0
			):
	"""
	Decorator for throttling a function. It is guaranteed that the execution
	of the function will take at least "execution_time". If the execution takes
	less than that, then the program will sleep for the difference between
	time elapsed in the function and "execution_time"

	If some sleeping is desired regardless of time spent inside the function
	decorated, you can use the parameter "sleep_at_least"

	If "random_factor" is defined, then the sleeping can take from
	(1 - random_factor) to (1 + random_factor) times as it would otherwise
	"""
	if sleep_at_least < 0.0 or random_factor > 1.0:
		raise ValueError
	
	def wrap_outer(func):
		def wrap_inner(*args, **kwargs):
			start = datetime.datetime.now()
			result = func(*args, **kwargs)
			end = datetime.datetime.now()
			elapsed_time = (end - start).total_seconds()

			time_to_sleep = max((execution_time - elapsed_time), sleep_at_least)
			r = (1.0 - random_factor) + 2.0 * random_factor * random.random()
			time.sleep(time_to_sleep * r)
			return result
		return wrap_inner
	return wrap_outer

class KillHandleTriggered(Exception):
	pass

class KillHandle(threading.Event):
	def check(self):
		if self.is_set():
			raise KillHandleTriggered

	def timeout(self, timeout_seconds: int):
		time.sleep(timeout_seconds)
		self.set()

class PreserveDirectory:
	def __init__(self):
		self.old_dir = None

	def __enter__(self):
		self.old_dir = os.getcwd()

	def __exit__(self, exc_type, exc_value, traceback):
		os.chdir(self.old_dir)

class UseDirectory:
	def __init__(
				self,
				go_to_directory: str,
				create_if_nonexistent: bool = True,
				create_parents: bool = True):
		self.go_to_directory = go_to_directory
		self.create_if_nonexistent = create_if_nonexistent
		self.create_parents = create_parents
		self.old_dir = None

	def __enter__(self):
		self.old_dir = os.getcwd()
		if not os.path.isdir(self.go_to_directory):
			if self.create_if_nonexistent:
				log.debug(f"Directory '{self.go_to_directory}' did not exist, creating")
				try:
					os.mkdir(self.go_to_directory)
				except FileNotFoundError:
					log.debug(f"Cannot reach '{self.go_to_directory}' directly")
					if self.create_parents:
						log.debug("Creating parent directories...")
						os.makedirs(self.go_to_directory)
					else:
						raise ValueError("Option create_parents was set to False")
			else:
				raise ValueError(f"Directory '{self.go_to_directory}' does not "
					"exist and we won't create it.")
		os.chdir(self.go_to_directory)

	def __exit__(self, exc_type, exc_value, traceback):
		os.chdir(self.old_dir)

def get_home_dir():
	home_dir = os.path.expanduser("~")
	if home_dir == "~":
		raise OSError("Failed to identify home directory")
	return home_dir

def find_files(regex: Union[str,re.Pattern] = None, join_path: str = None) -> Set[str]:
	current_path = os.getcwd()

	if regex is None:
		regex = ""
	
	if join_path is not None:
		path = os.path.join(current_path, join_path)
	else:
		path = current_path
	
	if type(regex) == re.Pattern:
		compiled = regex
	else:
		compiled = re.compile(regex, flags=re.I)
	
	files = set([
		f
		for f in os.listdir(path)
		if os.path.isfile(os.path.join(path, f)) and bool(compiled.search(f))
	])
	return files

def get_project_root_path() -> str:
	"""Finds realpath to the /src directory by going upwards in the directory
	tree and looking for the presence of "main.py"

	Can also identify if it is in a Docker container, by matching the full path.
	"""
	with PreserveDirectory():
		while True:
			files = [x for x in os.listdir() if os.path.isfile(x)]
			current_dir = os.getcwd()
			if ((os.path.basename(current_dir) == "src" 
					or current_dir == '/opt/scraper')
					and "main.py" in files):
				break 
			parent_dir = os.path.dirname(current_dir)
			if parent_dir == current_dir:
				raise ValueError("The project root could not be found!")
			os.chdir(os.path.pardir)
		return current_dir

def go_to_project_root():
	"""Changes working directory to the /src directory
	"""
	os.chdir(get_project_root_path())

def dirname_from_file(filename: str) -> str:
	return os.path.dirname(os.path.realpath(filename))