import datetime, time, random

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