import argparse

parser = argparse.ArgumentParser(
	description="Scrapes data from social media. Not all the options are "
	"valid for all the navigators (implementations). In doubt, please "
	"consult the documentation for the navigator.",
	epilog="Please observe the Terms and Conditions of the platform(s) before running this software."
)

# action="store_const", const=True
parser.add_argument(
	'--use_clean_profile',
	action='store_const',
	const=True,
	help='Does not reset data from last use when starting Chrome.'
)
parser.add_argument(
	'--no_clean_profile',
	action='store_const',
	dest="use_clean_profile",
	const=False,
	help='Does not reset data from last use when starting Chrome.'
)

print(parser.parse_args())