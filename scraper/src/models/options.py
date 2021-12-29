from typing import Optional

import argparse

class Options:
	scroll_limit: int
	timeout: int
	logging: int
	keep_logs: bool
	slow_mode: bool
	use_clean_profile: bool
	use_fake_user_agent: bool
	navigator_name: str

	scraping_interval: Optional[int]
	db_conn_string: Optional[str]
	account_name: Optional[str]
	credentials_file: Optional[str]

	def __init__(self, argparse_options: argparse.Namespace):
		options_dict = vars(argparse_options)
		for key, value in options_dict.items():
			setattr(self, key, value)

	def __str__(self):
		return str(vars(self))