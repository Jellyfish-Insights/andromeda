#!/usr/bin/env python3
from navigators.abstract import AbstractNavigator

class TestNavigator(AbstractNavigator):
	"""
	This is to be used for quick checks, prototyping etc.
	"""
	
	############################################################################
	# METHODS
	############################################################################
	def main(self):
		self.go("http://localhost:5000")