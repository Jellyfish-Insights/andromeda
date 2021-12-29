#!/usr/bin/env python3

import os, sys, time, shutil, threading, signal, random, traceback

import browsermobproxy, undetected_chromedriver.v2 as uc
from selenium.common.exceptions import WebDriverException, NoSuchWindowException

from logger import logger, change_logger_level
from arg_parser import parse
from models.options import Options
from libs.kill_handle import KillHandle, KillHandleTriggered

from db import DBError, setup_db
from navigators.abstract import AbstractNavigator, YouProbablyGotBlocked

################################################################################
# CONSTANTS
################################################################################

# Log file
UC_LOG_FILE = "uc.log"

# Options for our ChromeDriver
PROFILE_DIR = "chrome_profile"

CHROMEDRIVER_OPTIONS = [
	# Hopefully, using the settings below will disable other popups that could
	# disturb complete browser automation
	"--no-first-run",
	"--no-service-autorun",
	"--password-store=basic",
	"--disable-notifications",

	# For ignoring "untrusted proxy" errors
	"--ignore-ssl-errors=yes",
	"--ignore-certificate-errors",

	# For disabling /dev/shm usage (Docker Containers don't allocate a lot of
	# memory for that)
	"--disable-dev-shm-usage",

	# For disabling cache
	"--disk-cache-size=0",
]

CHROMEDRIVER_SERVICEARGS = ["--verbose", f"--log-path={UC_LOG_FILE}"]

# An exception is triggered and driver exits if page takes longer than that
# to load (given in seconds)
PAGE_LOAD_TIMEOUT = 30

COMMON_DISPLAY_RESOLUTIONS = [
	(1920, 1200),
	(1920, 1080),
	(1366, 768),
	(1536, 864),
	(1440, 900),
	(1366, 768),
	(1280, 800),
	(1280, 720),
	(1280, 1024),
	(1024, 768)
]

# This was not yet tested. The idea is to change the language preference header
# now and then, in order to avoid IP bans

# https://developer.chrome.com/docs/webstore/i18n/#localeTable
# https://stackoverflow.com/questions/52098821/selenium-webdriver-set-preferred-browser-language-de
# https://gist.github.com/traysr/2001377
LOCALE_OPTIONS = [
	'en', 'en-US', 'en-GB', 'fr', 'de', 'pl', 'nl', 'it', 'es', 'pt-BR',
	'zh-TW', 'ja', 'ko'
]
# Then use, in Driver configuration
# options.add_argument('--disable-translate')
# options.add_argument("--lang=de-DE")


################################################################################
# SCRAPER CLASS
################################################################################

class Scraper:
	def __init__(
			self,
			options: Options,
			nav_class: type
			):
		self.options = options
		self.kill_handle = KillHandle()

		if nav_class not in AbstractNavigator.__subclasses__():
			logger.critical("nav_class must be a subclass of AbstractNavigator!")
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
		logger.info("Starting proxy server...")

		self.server = browsermobproxy.Server(f"../browsermob-proxy-2.1.4/bin/browsermob-proxy")
		self.server.start()
		self.proxy = self.server.create_proxy()
		self.proxy.new_har(options=AbstractNavigator.HAR_OPTIONS)

	def start_driver(self) -> None:
		logger.info("Starting Undetected Chrome Driver...")

		options = uc.ChromeOptions()

		if self.options.use_clean_profile and os.path.isdir(PROFILE_DIR):
			shutil.rmtree(PROFILE_DIR)
		
		options.user_data_dir = PROFILE_DIR

		for opt in CHROMEDRIVER_OPTIONS:
			options.add_argument(opt)

		# Connecting driver to BrowserMob proxy
		options.add_argument(f'--proxy-server=localhost:{self.proxy.port}')


		# Another idea is to use a fake user agent:
		# https://stackoverflow.com/a/62520191/17030712
		# https://stackoverflow.com/questions/49565042/way-to-change-google-chrome-user-agent-in-selenium/49565254#49565254

		if self.options.use_fake_user_agent:
			from fake_useragent import UserAgent
			try:
				ua = UserAgent()
				userAgent = ua.random
				logger.info(f"Using fake user agent = {userAgent}")
				options.add_argument(f"--user-agent={userAgent}")
			except Exception as e:
				logger.warning("Could not use fake user agent.")
				logger.warning(e)

		try:
			self.driver = uc.Chrome(
				options=options,
				service_args=CHROMEDRIVER_SERVICEARGS
			)
		except WebDriverException as err:
			logger.critical("We could not open Chrome Driver. Cleaning up and exiting...")
			logger.critical(err)
			self.server.stop()
			sys.exit(1)
		except BaseException as err:
			logger.critical("An unknown error happened!")
			logger.critical(err)
			traceback.print_exc()
			self.server.stop()
			raise		

		# Will raise an exception if any page takes more than PAGE_LOAD_TIMEOUT
		# seconds to load
		self.driver.set_page_load_timeout(PAGE_LOAD_TIMEOUT)
		resolution = random.choice(COMMON_DISPLAY_RESOLUTIONS)
		self.driver.set_window_size(*resolution)
		logger.info(f"Using window resolution = {resolution}")

	def navigate_to_content(self) -> None:
		try:
			navigator: AbstractNavigator = self.nav_class(
					self.options,
					self.driver,
					self.proxy,
					logger,
					self.kill_handle
			)
		except (ValueError, AttributeError):
			self.cleanup(1)

		try:
			navigator.main()
		except NoSuchWindowException as err:
			logger.critical("Chrome window was closed from an outside agent!")
			logger.critical(err)
			traceback.print_exc()
			self.cleanup(1)
		except (KillHandleTriggered, YouProbablyGotBlocked, DBError):
			self.cleanup(1)
		except Exception as err:
			logger.critical("An unknown error happened:")
			logger.critical(err)
			traceback.print_exc()
			self.cleanup(1)

	def use_timeout(self):
		timeout_seconds = self.options.timeout
		if timeout_seconds == 0:
			return
		else:
			logger.debug(f"Using timeout of {timeout_seconds} seconds.")

		def stop_program(kill_handle: KillHandle):
			# We will change the sentinel value and hope that main thread
			# exits gracefully. If that fails, we will force termination

			# kill_handle.wait(timeout_seconds)
			kill_handle.timeout(timeout_seconds)
			logger.info("Kill handle was set. Exiting on next iteration.")

			time.sleep(timeout_seconds / 2)
			logger.warning("Process still hasn't exited, forcing cleanup.")
			self.cleanup()
		
		t = threading.Thread(
			target=stop_program,
			args=(self.kill_handle,),
			daemon=True
		)
		t.start()

	def cleanup(self, exit_code: int = 0) -> None:
		logger.info("Cleaning...")
		try:
			self.server.stop()
		except AttributeError:
			logger.debug("Server was not running yet, or had already been killed.")

		try:
			self.driver.quit()
		except AttributeError:
			logger.debug("Driver was not running yet, or had already been killed.")			

		if not self.options.keep_logs:
			# Removing log files created by BrowserMob and Undetected Chrome
			list_of_files = ["bmp.log", "server.log", UC_LOG_FILE]

			for file in list_of_files:
				if os.path.isfile(file):
					os.unlink(file)

		logger.info("Exiting...")
		sys.exit(exit_code)

################################################################################
# MAIN / DRIVER CODE
################################################################################

def main():
	setup_db()
	options = parse()

	# Configure logger
	change_logger_level(options.logging)

	logger.info("We will run the scraper with the following options:")
	logger.info(options)
	
	navigator = AbstractNavigator.select_navigator(options.navigator_name)
	scraper = Scraper(options, navigator)

	def sigterm_handle(signal_received, frame):
		logger.info(f"Process received signal = {signal_received}. Cleaning up and exiting.")
		scraper.cleanup(signal_received)

	signal.signal(signal.SIGINT, sigterm_handle)
	signal.signal(signal.SIGTERM, sigterm_handle)

	try:
		scraper.start()
		scraper.navigate_to_content()
		scraper.cleanup()
	except SystemExit as exit_code:
		logger.info(f"Exiting with code {exit_code}")
	except Exception as err:
		logger.critical("An unknown exception was raised.")
		logger.critical(err)
		traceback.print_exc()
		scraper.cleanup(1)

if __name__ == "__main__":
	main()
