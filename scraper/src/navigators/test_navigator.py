#!/usr/bin/env python3
from scraper_middleware import ScraperMiddleWare

class TestNavigator(ScraperMiddleWare):
	"""
	This is to be used for quick checks, prototyping etc.
	"""
	needs_authentication = False
	
	############################################################################
	# METHODS
	############################################################################
	def main(self):
		self.go("https://www.whatismybrowser.com/detect/what-http-headers-is-my-browser-sending")
		breakpoint()
		# For reference, my everyday browser gave the following message:
		# Your browser fingerprint appears to be unique among the 225,631 tested
		# in the past 45 days.
		# 
		# Currently, we estimate that your browser has a fingerprint that 
		# conveys at least 17.78 bits of identifying information.
		self.go("https://coveryourtracks.eff.org/kcarter?aat=1")
		breakpoint()
		self.go("https://amiunique.org/fp")
		breakpoint()
