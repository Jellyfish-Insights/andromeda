import argparse, os
from typing import Optional
from logger import logger

DEFAULT_SCROLL_LIMIT = 5
DEFAULT_TIMEOUT = 480
DEFAULT_SCRAPING_INTERVAL = 600
DEFAULT_DB_CONN_STR = "postgresql://brab:brickabode@localhost:5432"
DEFAULT_LOGGING_LEVEL = 10

class Options:
	scroll_limit: int
	timeout: int
	logging: int
	keep_logs: bool
	slow_mode: bool
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

def parse() -> Options:
	parser = argparse.ArgumentParser(
		description="Scrapes data from social media.",
		epilog="Please observe the Terms and Conditions of the platform(s) before running this software."
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
		help="(Parameter is only relevant for running as a container). Interval between each " +
			f"call to the scraper. Default value is {DEFAULT_SCRAPING_INTERVAL} seconds."
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
		help='Scrapes social media around 2 times slower'
	)
	parser.add_argument(
		'--account_name',
		'-a',
		action='store',
		type=str,
		# The scheduler can run without this option. For some of the scrapers,
		# though, it is needed
		required=False,
		help="Name of the social media account to be scraped, if appliable."
			"Accounts must start with an '@' in the services that use it."
	)
	parser.add_argument(
		'--credentials_file',
		'-c',
		action='store',
		type=str,
		# The scheduler can run without this option. For some of the scrapers,
		# though, it is needed
		required=False,
		help="Credentials file for authenticating to social media, if appliable."
	)

	# Positional argument
	parser.add_argument(
		'navigator_name',
		type=str,
		choices=["TikTok", "YouTube"],
		help="Name of the navigator to be used, i.e., TikTok, YouTube, Twitter, etc."
	)

	args = parser.parse_args()
	
	if args.scroll_limit < 0:
		logger.critical("scroll_limit must be a non-negative integer")
		exit(1)

	if args.timeout < 0:
		logger.critical("timeout must be a non-negative integer")
		exit(1)

	if args.scraping_interval < 10:
		logger.critical("You can't run the scraper more often than every 60 seconds!")
		exit(1)

	if args.credentials_file:
		if not (os.path.isfile(args.credentials_file)
				and os.access(args.credentials_file, os.R_OK)):
			logger.critical("File name is not a file to which you have read permissions.")
			exit(1)
		else:
			args.credentials_file = os.path.realpath(args.credentials_file)

	return Options(args)

parse()