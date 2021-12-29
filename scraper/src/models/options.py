from typing import Optional

import argparse

FILENAME = "scraper.py"

class Options:
	scroll_limit: int
	timeout: int
	logging: int
	keep_logs: bool
	slow_mode: bool
	use_clean_profile: bool
	use_fake_user_agent: bool
	use_random_window_size: bool
	use_random_locale: bool
	use_random_timezone: bool
	force_logout: bool	

	scraping_interval: Optional[int]
	db_conn_string: Optional[str]
	account_name: Optional[str]
	credentials_file: Optional[str]

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