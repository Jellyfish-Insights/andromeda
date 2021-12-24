import threading, time

class KillHandleTriggered(Exception):
	pass

class KillHandle(threading.Event):
	def check(self):
		if self.is_set():
			raise KillHandleTriggered

	def timeout(self, timeout_seconds: int):
		time.sleep(timeout_seconds)
		self.set()