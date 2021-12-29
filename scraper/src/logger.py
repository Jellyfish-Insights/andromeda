import __main__
import logging, os

class CustomLogger(logging.Logger):
	initialized_object = None

	def __new__(cls):
		if cls.initialized_object is None:
			logger = logging.getLogger(os.path.basename(__main__.__file__))
			logger.setLevel(logging.DEBUG)

			ch = logging.StreamHandler()
			ch.setLevel(logging.DEBUG)

			formatter = logging.Formatter('[%(asctime)s] %(name)s - %(levelname)s : %(message)s')
			ch.setFormatter(formatter)
			logger.addHandler(ch)

			# If you don't set this to true, messages propagate across many logger levels,
			# generating duplicate messages
			logger.propagate = False
			cls.initialized_object = logger

		return cls.initialized_object
	
	def __init__(self):
		"""Creates a custom logger according to options defined in source code"""

	def change_logger_level(self, level: int):
		for handler in self.handlers:
			handler.setLevel(level)