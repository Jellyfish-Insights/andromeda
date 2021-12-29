#!/usr/bin/env python3

import os, sys, time, shutil, threading, signal, traceback, random
from typing import Dict

import browsermobproxy, undetected_chromedriver.v2 as uc
from selenium.common.exceptions import WebDriverException, NoSuchWindowException
from fake_useragent import UserAgent

from logger import log, change_logger_level
from arg_parser import parse
from models.options import Options
from libs.kill_handle import KillHandle, KillHandleTriggered

from db import DBError, setup_db
from defaults import anonymization, chrome
from navigators.abstract import AbstractNavigator, YouProbablyGotBlocked

class Scraper:
	def __init__(
			self,
			options: Options,
			nav_class: type
			):
		self.options = options
		self.kill_handle = KillHandle()

		if nav_class not in AbstractNavigator.__subclasses__():
			log.critical("nav_class must be a subclass of AbstractNavigator!")
			exit(1)
		
		self.nav_class = nav_class

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
		chrome_options = uc.ChromeOptions()

		if self.options.use_clean_profile and os.path.isdir(chrome.PROFILE_DIR):
			shutil.rmtree(chrome.PROFILE_DIR)
		
		chrome_options.user_data_dir = chrome.PROFILE_DIR

		for opt in chrome.CHROMEDRIVER_OPTIONS:
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
				service_args=chrome.CHROMEDRIVER_SERVICEARGS
			)
		except WebDriverException as err:
			log.critical("We could not open Chrome Driver. Cleaning up and exiting...")
			log.critical(err)
			self.server.stop()
			sys.exit(1)
		except BaseException as err:
			log.critical("An unknown error happened!")
			log.critical(err)
			traceback.print_exc()
			self.server.stop()
			raise		

		# Will raise an exception if any page takes more than PAGE_LOAD_TIMEOUT
		# seconds to load
		self.driver.set_page_load_timeout(chrome.PAGE_LOAD_TIMEOUT)

		if self.options.use_random_window_size:
			self.random_window_size()
		else:
			log.info("Starting maximized")
			self.driver.maximize_window()

	def navigate_to_content(self) -> None:
		try:
			navigator: AbstractNavigator = self.nav_class(
					self.options,
					self.driver,
					self.proxy,
					self.kill_handle
			)
		except (ValueError, AttributeError) as e:
			log.critical("An error occurred initializing navigator:")
			log.critical(e)
			self.cleanup(1)

		try:
			navigator.main()
		except NoSuchWindowException as err:
			log.critical("Chrome window was closed from an outside agent!")
			log.critical(err)
			traceback.print_exc()
			self.cleanup(1)
		except (KillHandleTriggered, YouProbablyGotBlocked, DBError):
			self.cleanup(1)
		except Exception as err:
			log.critical("An unknown error happened:")
			log.critical(err)
			traceback.print_exc()
			self.cleanup(1)

	def use_timeout(self):
		timeout_seconds = self.options.timeout
		if timeout_seconds == 0:
			return
		else:
			log.debug(f"Using timeout of {timeout_seconds} seconds.")

		def stop_program(kill_handle: KillHandle):
			# We will change the sentinel value and hope that main thread
			# exits gracefully. If that fails, we will force termination

			# kill_handle.wait(timeout_seconds)
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
		log.info("Cleaning...")
		try:
			self.proxy.close()
		except Exception as e:
			log.debug("Error closing proxy:")
			log.debug(e)

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
			list_of_files = ["bmp.log", "server.log", chrome.UC_LOG_FILE]

			for file in list_of_files:
				if os.path.isfile(file):
					os.unlink(file)

		log.info("Exiting...")
		sys.exit(exit_code)

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
			while first_pass or (random.random() < 0.8 and q >= 0.1):
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

	def do_not_track():
		# Launch Chrome with "Do Not Track" setting. There's currently no easy way
		# to do this. One possibility we might want to try later is navigating
		# to "chrome://settings/cookies" and controlling the setting.
		pass

################################################################################
# MAIN / DRIVER CODE
################################################################################

def main():
	setup_db()
	options = parse()
	change_logger_level(options.logging)

	log.info("We will run the scraper with the following options:")
	log.info(options)
	log.info("You can run with the same options with the following command:")
	log.info(options.generate_cmd())
	
	navigator = AbstractNavigator.select_navigator(options.navigator_name)
	scraper = Scraper(options, navigator)

	def sigterm_handle(signal_received, frame):
		log.info(f"Process received signal = {signal_received}. Cleaning up and exiting.")
		scraper.cleanup(signal_received)

	signal.signal(signal.SIGINT, sigterm_handle)
	signal.signal(signal.SIGTERM, sigterm_handle)

	try:
		scraper.start()
		scraper.navigate_to_content()
		scraper.cleanup()
	except SystemExit as exit_code:
		log.info(f"Exiting with code {exit_code}")
	except Exception as err:
		log.critical("An unknown exception was raised.")
		log.critical(err)
		traceback.print_exc()
		scraper.cleanup(1)

if __name__ == "__main__":
	main()
