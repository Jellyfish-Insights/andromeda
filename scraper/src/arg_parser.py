import argparse
from scraper_middleware import ScraperMiddleWare
from models.options import Options

DEFAULT_DB_CONN_STR = "postgresql://brab:brickabode@localhost:5432"
DEFAULT_LOGGING_LEVEL = 10

def parse() -> Options:
	navigators_dict = ScraperMiddleWare.get_available_navigators()

	parser = argparse.ArgumentParser(
		description="Scrapes data from social media. Not all the options are "
		"valid for all the navigators (implementations). In doubt, please "
		"consult the documentation for the navigator.",
		epilog="Please observe the Terms and Conditions of the platform(s) before running this software."
	)

	# action="store"
	parser.add_argument(
		'--logging',
		'-g',
		action='store',
		type=int,
		choices=[10, 20, 30, 40, 50],
		default=DEFAULT_LOGGING_LEVEL,
		help="Defines how much of the messages should be printed to the screen. "
			"Accepted values are 10, 20, 30, 40, 50. Default is to log "
			"everything, from DEBUG to CRITICAL."
	)

	# action="store_true"
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

	# action="store_const", const=True or False
	parser.add_argument(
		'--use_disposable_profile',
		action='store_const',
		const=True,
		help='If set, Chrome will run with a throw-away profile.'
	)
	parser.add_argument(
		'--no_disposable_profile',
		action='store_const',
		dest="use_disposable_profile",
		const=False,
		help='Chrome will use the latest available profile, or create one if there is none.'
	)


	parser.add_argument(
		'--use_fake_user_agent',
		action='store_const',
		const=True,
		help='Uses a fake user agent to avoid bot detection.'
	)
	parser.add_argument(
		'--no_fake_user_agent',
		action='store_const',
		dest="use_fake_user_agent",
		const=False,
		help="Don't use a fake user agent."
	)

	parser.add_argument(
		'--use_random_window_size',
		action='store_const',
		const=True,
		help='Uses random window size to avoid detection.'
	)
	parser.add_argument(
		'--no_random_window_size',
		action='store_const',
		dest="use_random_window_size",
		const=False,
		help="Don't use random window size to avoid detection. Start maximized."
	)


	parser.add_argument(
		'--use_random_locale',
		action='store_const',
		const=True,
		help='Uses random locale to avoid detection.'
	)
	parser.add_argument(
		'--no_random_locale',
		action='store_const',
		dest="use_random_locale",
		const=False,
		help="Don't use a random locale."
	)


	parser.add_argument(
		'--use_random_timezone',
		action='store_const',
		const=True,
		help='Uses random timezone to avoid detection.'
	)
	parser.add_argument(
		'--no_random_timezone',
		action='store_const',
		dest="use_random_timezone",
		const=False,
		help="Don't use a random timezone."
	)


	parser.add_argument(
		'--force_logout',
		action='store_const',
		const=True,
		help='Forces logging out from accounts, in case a logged in account is detected.'
	)
	parser.add_argument(
		'--no_force_logout',
		action='store_const',
		dest="force_logout",
		const=False,
		help="Don't log out of accounts if signed in."
	)

	# Bulk options, will set the above if they are unset
	parser.add_argument(
		'--use_anonymous',
		'-a',
		action='store_const',
		const=True,
		help="Use all anonymous options which haven't been set as a CLI option."
	)
	parser.add_argument(
		'--no_use_anonymous',
		action='store_const',
		dest="use_anonymous",
		const=False,
		help="Disable all anonymous options which haven't been set as a CLI option."
	)

	# Optional arguments
	parser.add_argument(
		'--scroll_limit',
		'-l',
		action='store',
		type=int,
		help='Defines how far down the scraper should scroll at most. '
	)
	parser.add_argument(
		'--timeout',
		'-t',
		action='store',
		type=int,
		help='Maximum time the program should run (estimated), given in '
			f"seconds. A value of zero will be considered as no timeout."
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
		'--account_name',
		action='store',
		type=str,
		help="Name of the social media account to be scraped, if appliable."
			"Accounts must start with an '@' in the services that use it."
	)
	parser.add_argument(
		'--password_encrypted',
		action='store',
		type=str,
		help="Password for the account_name provided, symmetrically encrypted "
			"with passphrase in source code."
	)
	parser.add_argument(
		'--password_plain',
		action='store',
		type=str,
		help="Password for the account_name provided, ready to use."
	)
	parser.add_argument(
		'--credentials_file',
		'-c',
		action='store',
		type=str,
		help="Credentials file for authenticating to social media, if appliable."
	)

	# Positional argument -- REQUIRED
	parser.add_argument(
		'navigator_name',
		type=str,
		choices=navigators_dict.keys(),
		help="Name of the navigator to be used, i.e., TikTok, YouTube, Twitter, etc."
	)

	### 

	args = parser.parse_args()
	navigator_class: ScraperMiddleWare = navigators_dict[args.navigator_name]
	options = Options.init_from_dict(vars(args))

	if args.use_anonymous is True:
		options.enable_anonymization_options()
	elif args.use_anonymous is False:
		options.disable_anonymization_options()

	needs_authentication = navigator_class.needs_authentication
	if needs_authentication:
		options.disable_anonymization_options()
	else:
		options.enable_anonymization_options()

	navigator_default_options = navigator_class.navigator_default_options
	options.apply_navigator_default_options(navigator_default_options)

	return options
