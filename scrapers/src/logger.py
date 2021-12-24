import __main__
import logging, os

# create logger
logger = logging.getLogger(os.path.basename(__main__.__file__))
logger.setLevel(logging.DEBUG)

# create console handler and set level to debug
ch = logging.StreamHandler()
ch.setLevel(logging.DEBUG)

# create formatter
formatter = logging.Formatter('[%(asctime)s] %(name)s - %(levelname)s : %(message)s')

# add formatter to ch
ch.setFormatter(formatter)

# add ch to logger
logger.addHandler(ch)

# If you don't set this to true, messages propagate across many logger levels,
# generating duplicate messages
logger.propagate = False

def change_logger_level(level: int):
	for handler in logger.handlers:
		handler.setLevel(level)