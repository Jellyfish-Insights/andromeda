import argparse
from typing import List

from logger import logger
from models.account_name import AccountName

DEFAULT_SCROLL_LIMIT = 5
DEFAULT_TIMEOUT = 480
DEFAULT_SCRAPING_INTERVAL = 600
DEFAULT_DB_CONN_STR = "postgresql://brab:brickabode@localhost:5432"
DEFAULT_LOGGING_LEVEL = 10

PROGRAM_INVOKATION = "python3 /opt/undetected-tiktok/scraper.py"

START_SCRAPER_SCRIPT = 'start_scraper.sh'

def parse(check_account_name = False) -> dict:
	parser = argparse.ArgumentParser(
		description="Scrapes data from videos from TikTok.",
		epilog="Please observe the Terms and Conditions of the platform before running this software."
	)

	# Optional arguments
	parser.add_argument(
		'--scroll_limit',
		'-l',
		action='store',
		type=int,
		default=DEFAULT_SCROLL_LIMIT,
		help='Defines how far down the scraper should scroll at most. ' +
			f"Default value is {DEFAULT_SCROLL_LIMIT} scrollings."
	)
	parser.add_argument(
		'--timeout',
		'-t',
		action='store',
		type=int,
		default=DEFAULT_TIMEOUT,
		help='Maximum time the program should run (estimated), given in '
			f"seconds. Default value is {DEFAULT_TIMEOUT} seconds. A value of "
			"zero is considered as no timeout."
	)
	parser.add_argument(
		'--scraping_interval',
		'-i',
		action='store',
		type=int,
		default=DEFAULT_SCRAPING_INTERVAL,
		help="(Parameter is only valid for scheduler.py, otherwise it will be ignored). "
			"Interval between each call to the scraper. Default value is "
			f"{DEFAULT_SCRAPING_INTERVAL} seconds."
	)
	parser.add_argument(
		'--db_conn_string',
		'-d',
		action='store',
		type=str,
		default=DEFAULT_DB_CONN_STR,
		help="Connection string for the database. Format is "
			"'postgresql://user:password@host:port'"
	)
	parser.add_argument(
		'--logging',
		'-g',
		action='store',
		type=int,
		default=DEFAULT_LOGGING_LEVEL,
		choices=[0, 10, 20, 30, 40, 50],
		help="Defines how much of the messages should be printed to the screen. "
			"Accepted values are 0, 10, 20, 30, 40, 50."
	)
	parser.add_argument(
		'--keep_logs',
		'-k',
		action='store_true',
		help='Enables logging for ChromeDriver and BrowserMob, helpful for debugging'
	)
	parser.add_argument(
		'--slow_mode',
		'-s',
		action='store_true',
		help='Scrapes TikTok around 2 times slower'
	)
	parser.add_argument(
		'--quiet',
		'-q',
		action='store_true',
		help="Don't print logging messages to the screen"
	)

	# Required arguments
	parser.add_argument(
		'account_name',
		type=str,
		help="Name of the TikTok account to be scraped. Should start with an '@'"
	)
	parser.add_argument(
		'navigator_name',
		type=str,
		choices=["TikTok", "YouTube"],
		help="Name of the navigator to be used, i.e., TikTok, YouTube, Twitter, etc."
	)

	args = parser.parse_args()
	
	if check_account_name:
		try:
			AccountName.test(args.account_name)
		except ValueError:
			logger.critical("Account name has invalid format. Must start with '@'")
			exit(1)

	if args.scroll_limit <= 0:
		logger.critical("scroll_limit must be a strictly positive integer")
		exit(1)

	if args.timeout < 0:
		logger.critical("timeout must be a non-negative integer")
		exit(1)

	if args.scraping_interval < 10:
		logger.critical("You can't run the scraper more often than every 60 seconds!")
		exit(1)

	return args

if __name__ == "__main__":
	opts = parse()
	print(opts.quiet)
	print(opts.timeout)
	print(opts.account_name)