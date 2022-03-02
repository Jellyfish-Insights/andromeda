#!/usr/bin/env python3

import signal
import traceback
import sys
from typing import Type, Union

from logger import log, change_logger_level
from arg_parser import parse
from db import setup_db
from scraper_core import BadArguments
from scraper_middleware import ScraperMiddleWare

def main():
	setup_db()
	options = parse()
	change_logger_level(options.logging)

	log.info("We will run the scraper with the following options:")
	log.info(options)
	log.info("You can run with the same options with the following command:")
	log.info(options.generate_cmd())
	
	scraper_middleware_class: Union[Type, None] = \
			ScraperMiddleWare.match_navigator_name(options.navigator_name)
	if scraper_middleware_class is None:
		return
	
	try:
		scraper: ScraperMiddleWare = scraper_middleware_class(options)
	except BadArguments:
		return

	def sigterm_handle(signal_received, frame):
		log.info(f"Process received signal = {signal_received}. Cleaning up and exiting.")
		scraper.cleanup(signal_received)

	signal.signal(signal.SIGINT, sigterm_handle)
	signal.signal(signal.SIGTERM, sigterm_handle)

	error_code = 0
	try:
		scraper.start()
	except SystemExit as exit_code:
		log.info(f"Exiting with code {exit_code}")
		sys.exit(exit_code)
	except Exception as err:
		log.critical("An unknown exception was raised.")
		log.critical(err)
		traceback.print_exc()
		error_code = 1
	finally:
		scraper.cleanup(error_code)


if __name__ == "__main__":
	main()
