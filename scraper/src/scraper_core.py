import os
import sys
import time
import shutil
import threading
import traceback
import random
from typing import Dict, Optional
import browsermobproxy, undetected_chromedriver.v2 as uc
from selenium.common.exceptions import WebDriverException, SessionNotCreatedException
from fake_useragent import UserAgent

from defaults import core as core_defaults, anonymization
from logger import log
from tools import KillHandle, KillHandleTriggered
from models.options import Options

################################################################################
# CUSTOM EXCEPTIONS
################################################################################

class ScraperException(Exception):
	"""Defines a base class for scraper exceptions"""

class BadArguments(ScraperException):
	pass

class EndOfPage(ScraperException):
	pass

class ElementNotFound(ScraperException):
	pass

class YouProbablyGotBlocked(ScraperException):
	"""
	We are as careful as possible not to get blocked, but sometimes it happens.
	Workarounds can be accessing through a proxy, waiting until you are unblocked
	or changing your scraping routine.
	"""
	pass

class JSException(ScraperException):
	pass

################################################################################
# CLASS DEFINITION
################################################################################

class ScraperCore:
	def __init__(self, options: Options):
		self.driver: Optional[uc.Chrome] = None
		self.proxy: Optional[browsermobproxy.Client] = None
		self.server: Optional[browsermobproxy.Server] = None
		self.options: Options = options
		self.kill_handle: KillHandle = KillHandle()
		self.cleaned_up: bool = False
		self.profile_dir: str = self.get_profile_dir()

	def start(self):
		self.use_timeout()
		self.use_source_dir()
		self.start_proxy()
		self.start_driver()
		try:
			self.kill_handle.check()
		except KillHandleTriggered:
			self.cleanup()

	@staticmethod
	def use_source_dir():
		# Move to folder where script is located, it will look for relative paths
		dirname = os.path.dirname(os.path.realpath(__file__))
		os.chdir(dirname)

	def start_proxy(self) -> None:
		log.info("Starting proxy server...")
		self.server = browsermobproxy.Server(f"../browsermob-proxy-2.1.4/bin/browsermob-proxy")
		self.server.start()
		self.proxy = self.server.create_proxy()

	def start_driver(self) -> None:
		log.info("Starting Undetected Chrome Driver...")
		chrome_options: uc.ChromeOptions = uc.ChromeOptions()
		chrome_options.user_data_dir = self.profile_dir
		log.info(f"Using profile directory = '{self.profile_dir}'")
		for opt in core_defaults.CHROMEDRIVER_OPTIONS:
			chrome_options.add_argument(opt)

		# Connecting driver to BrowserMob proxy
		chrome_options.add_argument(f'--proxy-server=localhost:{self.proxy.port}')

		if self.options.use_fake_user_agent:
			self.fake_user_agent(chrome_options)
		if self.options.use_random_locale:
			self.fake_locale()
		if self.options.use_random_timezone:
			self.fake_timezone()

		try:
			self.driver = uc.Chrome(
				options=chrome_options,
				service_args=core_defaults.CHROMEDRIVER_SERVICEARGS
			)
		except SessionNotCreatedException as err:
			log.critical("Session was not created! Check if ChromeDriver version "
				"is compatible. You might want to run `sudo apt update && sudo "
				"apt upgrade -y`")
			log.critical(err)
			traceback.print_exc()
			self.server.stop()
			sys.exit(1)
		except (WebDriverException, KillHandleTriggered) as err:
			log.critical("We could not open Chrome Driver. Cleaning up and exiting...")
			log.critical(err)
			traceback.print_exc()
			self.server.stop()
			sys.exit(1)
		except Exception as err:
			log.critical("An unknown error happened!")
			log.critical(err)
			traceback.print_exc()
			self.server.stop()
			raise		

		# Will raise an exception if any page takes more than PAGE_LOAD_TIMEOUT
		# seconds to load
		self.driver.set_page_load_timeout(core_defaults.PAGE_LOAD_TIMEOUT)

		if self.options.use_random_window_size:
			self.random_window_size()
		else:
			log.info("Starting maximized")
			self.driver.maximize_window()

	def use_timeout(self):
		timeout_seconds = self.options.timeout
		if timeout_seconds == 0:
			return
		else:
			log.debug(f"Using timeout of {timeout_seconds} seconds.")

		def stop_program(kill_handle: KillHandle):
			# We will change the sentinel value and hope that main thread
			# exits gracefully. If that fails, we will force termination
			kill_handle.timeout(timeout_seconds)
			log.info("Kill handle was set. Exiting on next iteration.")

			time.sleep(timeout_seconds / 2)
			log.warning("Process still hasn't exited, forcing cleanup.")
			self.cleanup()
		
		t = threading.Thread(
			target=stop_program,
			args=(self.kill_handle,),
			daemon=True
		)
		t.start()

	def cleanup(self, exit_code: int = 0) -> None:
		if self.cleaned_up:
			return
		
		log.info("Cleaning...")
		try:
			self.proxy.close()
		except AttributeError:
			log.debug("Proxy had not been established yet.")

		try:
			self.server.stop()
		except AttributeError:
			log.debug("Server was not running yet, or had already been killed.")

		try:
			self.driver.quit()
		except AttributeError:
			log.debug("Driver was not running yet, or had already been killed.")			

		if not self.options.keep_logs:
			# Removing log files created by BrowserMob and Undetected Chrome
			list_of_files = ["bmp.log", "server.log", core_defaults.UC_LOG_FILE]

			for file in list_of_files:
				if os.path.isfile(file):
					os.unlink(file)

		if os.path.isdir(self.profile_dir) and self.options.use_disposable_profile:
			shutil.rmtree(self.profile_dir, ignore_errors=True)

		self.cleaned_up = True
		log.info("Exiting...")
		sys.exit(exit_code)

	def get_profile_dir(self) -> str:
		prefix = core_defaults.PROFILE_DIR
		throwaway_suffix = "throwaway"
		throwaway_dirname = f"{prefix}___{throwaway_suffix}"
		
		if os.path.isdir(throwaway_dirname):
			shutil.rmtree(throwaway_dirname, ignore_errors=True)
		
		if self.options.use_disposable_profile:
			log.debug(f"Using disposable profile '{throwaway_dirname}'")
			os.mkdir(throwaway_dirname)
			return throwaway_dirname

		existing_profiles = sorted([
			x
			for x in os.listdir()
			if os.path.isdir(x) and x.startswith(prefix)
		])
		if not existing_profiles:
			new_dirname = f"{prefix}___{int(time.time())}"
			log.debug(f"No existing profile. Will use {new_dirname}")
			os.mkdir(new_dirname)
			return new_dirname
		else:
			existing_profile = existing_profiles[-1]
			log.debug(f"Found existing profile {existing_profile}")
			return existing_profile


	############################################################################
	# METHODS FOR ANONYMIZATION
	############################################################################

	def fake_timezone(self):
		tz = random.choice(anonymization.TIMEZONES)
		log.info(f"Using timezone = {tz}")
		os.environ["TZ"] = tz

	def unset_via_header(self):
		# BrowserMobProxy sets a custom "Via" header that looks very suspicious
		# Unfortunately there's currently no way to unset it other than recompiling
		# the source for one of its dependencies. See:
		# https://stackoverflow.com/a/65712127/17030712
		# See line 274
		# https://github.com/adamfisk/LittleProxy/blob/6e0d253935b0694a23b40580bb72599c279deb08/src/main/java/org/littleshoot/proxy/impl/ProxyUtils.java
		pass

	def set_header(self, header: Dict[str, str]):
		log.debug(f"Setting header = {header}")
		status = self.proxy.headers(header)
		if status != 200:
			log.warning("Server replied with non-200 status")

	def fake_user_agent(self, chrome_options: uc.ChromeOptions):
		for header in anonymization.FAKE_UA_UNSET_HEADERS:
			self.set_header({header: ""})
		log.info("Using fake user agent as a Chrome argument was deprecated "
			"due to inadvertently disabling JavaScript.")
		return
		try:
			ua = UserAgent()
			userAgent = ua.random
			log.info(f"Using fake user agent = {userAgent}")
			chrome_options.add_argument(f"--user-agent={userAgent}")
		except Exception as e:
			log.warning("Could not use fake user agent.")
			log.warning(e)

	def fake_locale(self):
		def create_fake_lang_header():
			languages = []

			first_pass = True
			q = 0.9
			while first_pass or (random.random() < 0.7 and q >= 0.1):
				first_pass = False

				language = random.choice(anonymization.LOCALE_OPTIONS)
				languages.append(f"{language};q={q:.1f}")
				q -= 0.1

			return ",".join(languages)
		
		header_value = create_fake_lang_header()
		self.set_header({"Accept-Language": header_value})

	def random_window_size(self):
		resolution = random.choice(anonymization.COMMON_DISPLAY_RESOLUTIONS)
		self.driver.set_window_size(*resolution)
		log.info(f"Using window resolution = {resolution}")

	def do_not_track(self):
		# Launch Chrome with "Do Not Track" setting. There's currently no easy way
		# to do this. One possibility we might want to try later is navigating
		# to "chrome://settings/cookies" and controlling the setting.
		pass