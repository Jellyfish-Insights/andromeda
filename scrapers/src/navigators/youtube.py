#!/usr/bin/env python3
import json, re
import selenium

from navigators.abstract import AbstractNavigator

class YouTube(AbstractNavigator):
	############################################################################
	# METHODS
	############################################################################
	def build_url(self):
		return f"https://www.youtube.com"

	def action_load(self):
		pass

	def action_interact(self):
		pass