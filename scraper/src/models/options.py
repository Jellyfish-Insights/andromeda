import os
from dataclasses import dataclass
from typing import Any, Dict, List

from logger import log

SCRIPT_INTERPRETER = "python3"
SCRIPT_FILENAME = "main.py"

@dataclass
class Options:
	# Default = DEBUG
	logging: int = None
	# These default to False
	keep_logs: bool = False
	slow_mode: bool = False
	# These default to None, but, if nulled, they can be bulk turned on or off
	# by Navigator according to setting needs_authentication
	use_disposable_profile: bool = None
	use_fake_user_agent: bool = None
	use_random_window_size: bool = None
	use_random_locale: bool = None
	use_random_timezone: bool = None
	force_logout: bool = None
	# Following variables default to None and it is the Navigators' duty
	# to set default values and validate
	scroll_limit: int = None
	timeout: int = None
	db_conn_string: str = None
	account_name: str = None
	password_encrypted: str = None
	password_plain: str = None
	credentials_file: str = None
	# This is actually required
	navigator_name: str = None

	############################################################################

	@property
	def anonymization_fields(self) -> List[str]:
		return [
			"use_disposable_profile",
			"use_fake_user_agent",
			"use_random_window_size",
			"use_random_locale",
			"use_random_timezone"
		]

	############################################################################

	@property
	def optional_fields(self) -> List[str]:
		return [
			"scroll_limit",
			"timeout",
			"logging",
			"db_conn_string",
			"account_name",
			"password_encrypted",
			"password_plain",
			"credentials_file"
		]

	@property
	def flag_fields(self) -> List[str]:
		return [
			"keep_logs",
			"slow_mode",
			"use_disposable_profile",
			"use_fake_user_agent",
			"use_random_window_size",
			"use_random_locale",
			"use_random_timezone",
			"force_logout"
		]

	@property
	def mandatory_fields(self) -> List[str]:
		return [ "navigator_name" ]

	############################################################################
	def __post_init__(self):
		from scraper_middleware import ScraperMiddleWare
		
		if self.navigator_name is None:
			raise ValueError("Navigator name must be provided")

		nav_names = ScraperMiddleWare.get_available_navigators().keys()
		if self.navigator_name not in nav_names:
			raise ValueError("Navigator name invalid")

		# It is better to do this check here and already resolve paths to realpaths
		# If we delegate this task to the navigator(s), pwd can be different and
		# cause trouble
		if self.credentials_file:
			if not (os.path.isfile(self.credentials_file)
					and os.access(self.credentials_file, os.R_OK)):
				log.critical("File name is not a file to which you have read permissions.")
				raise ValueError
			else:
				self.credentials_file = os.path.realpath(self.credentials_file)

	def apply_navigator_default_options(self, defaults_dict):
		"""If the keys provided in "defaults_dict" are set to None in this object,
		then we set those keys to their value in "defaults_dict".
		"""
		for key in defaults_dict:
			if not hasattr(self, key):
				log.debug(f'"defaults_dict" provided unknown key "{key}", ignoring.')
				continue
			if getattr(self, key) is None:
				value = defaults_dict[key]
				log.debug(f"Parameter {key} was not provided, setting to {value}")
				setattr(self, key, value)

	def enable_anonymization_options(self):
		log.debug("Setting anonymization options to True where they are undefined.")
		for field in self.anonymization_fields:
			if getattr(self, field) is None:
				setattr(self, field, True)

	def disable_anonymization_options(self):
		log.debug("Setting anonymization options to False where they are undefined.")
		for field in self.anonymization_fields:
			if getattr(self, field) is None:
				setattr(self, field, False)

	@property
	def anonymization_options(self) -> List[Any]:
		return [getattr(self, field) for field in self.anonymization_fields]

	@classmethod
	def init_from_dict(cls, options_dict: Dict[str,Any]) -> "Options":
		extra_options_removed = {
			key: options_dict[key]
			for key in options_dict
			if hasattr(cls, key)
		}
		return Options(**extra_options_removed)

	def generate_cmd(
			self,
			script_interpreter: str = SCRIPT_INTERPRETER,
			script_filename: str = SCRIPT_FILENAME
			) -> str:
		cmd = f"{script_interpreter} {script_filename} "
		for flag in self.flag_fields:
			if vars(self)[flag]:
				cmd += f"--{flag} "
		for opt in self.optional_fields:
			if vars(self)[opt]:
				cmd += f"--{opt}='{vars(self)[opt]}' "
		for opt in self.mandatory_fields:
			cmd += f"{vars(self)[opt]} "
		
		return cmd