from typing import Optional

import argparse

FILENAME = "scraper.py"

class Options:
	# Default = DEBUG
	logging: int
	# These default to False
	keep_logs: bool
	slow_mode: bool
	# These default to None, but, if nulled, they can be bulk turned on or off
	# by Navigator according to setting needs_authentication
	use_clean_profile: Optional[bool]
	use_fake_user_agent: Optional[bool]
	use_random_window_size: Optional[bool]
	use_random_locale: Optional[bool]
	use_random_timezone: Optional[bool]
	force_logout: Optional[bool]
	# Following variables default to None and it is the Navigators' duty
	# to set default values and validate
	scroll_limit: Optional[int]
	timeout: Optional[int]
	db_conn_string: Optional[str]
	account_name: Optional[str]
	credentials_file: Optional[str]
	# This is actually required
	navigator_name: str

	@property
	def anonymization_options(self):
		return [
			self.use_clean_profile,
			self.use_fake_user_agent,
			self.use_random_window_size,
			self.use_random_locale,
			self.use_random_timezone
		]

	def __init__(self, argparse_options: argparse.Namespace):
		options_dict = vars(argparse_options)
		for key, value in options_dict.items():
			setattr(self, key, value)

	def __str__(self):
		return str(vars(self))

	def generate_cmd(self):
		options = [
			"scroll_limit",
			"timeout",
			"logging"
		]
		flags = [
			"keep_logs",
			"slow_mode",
			"use_clean_profile",
			"use_fake_user_agent",
			"use_random_window_size",
			"use_random_locale",
			"use_random_timezone",
			"force_logout"
		]
		nullable = [
			"scraping_interval",
			"db_conn_string",
			"account_name",
			"credentials_file"
		]
		mandatory = [ "navigator_name" ]

		cmd = f"{FILENAME} "
		for opt in options:
			cmd += f"--{opt}='{vars(self)[opt]}' "
		for flag in flags:
			if vars(self)[flag]:
				cmd += f"--{flag} "
		for opt in nullable:
			if vars(self)[opt]:
				cmd += f"--{opt}='{vars(self)[opt]}' "
		for opt in mandatory:
			cmd += f"{vars(self)[opt]} "
		
		return cmd