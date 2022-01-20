import __main__
import logging, os

log = logging.getLogger(os.path.basename(__main__.__file__))
log.setLevel(logging.DEBUG)

ch = logging.StreamHandler()
ch.setLevel(logging.DEBUG)

formatter = logging.Formatter('[%(asctime)s] %(name)s - %(levelname)s : %(message)s')
ch.setFormatter(formatter)
log.addHandler(ch)

# If you don't set this to true, messages propagate across many logger levels,
# generating duplicate messages
log.propagate = False

def change_logger_level(level: int):
	for handler in log.handlers:
		handler.setLevel(level)
