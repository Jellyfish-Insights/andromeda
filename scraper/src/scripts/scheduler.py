#!/usr/bin/env python3
import re, sys, os, random

from arg_parser import parse
from models.account_name import AccountName
from db import setup_db

SCRAPER_SCRIPT = "scraper.py"

def main():
	setup_db()
	options = parse()
	
	os.chdir(os.path.dirname(os.path.realpath(__file__)))
	with open("to_scrape.sh", "w") as fp:
		fp.write("#!/usr/bin/env bash\n")
		command = " ".join(sys.argv)

		distinct_accounts = [x.account_name for x in list(AccountName.get_all())]
		if len(distinct_accounts) == 0:
			print("We are not currently following any accounts. "
				"Please INSERT into \"account_name\" table to start.")
			return

		# At each pass, we will follow a random order. Hopefully, this will help
		# get data in a homogeneous way for all accounts, while not getting us
		# blocked
		random.shuffle(distinct_accounts)
		print(f"The following accounts were found: {distinct_accounts}")
		
		for account in distinct_accounts:
			filename = os.path.basename(__file__)
			edited_command = re.sub(filename, SCRAPER_SCRIPT, command)
			fp.write(f"python3 {edited_command} --account_name={account}\n")

			# A lot of java processes are left dangling for no reason
			# This is still an open issue by BrowserMob Proxy Py
			# https://github.com/AutomatedTester/browsermob-proxy-py/issues/8
			fp.write("killall -9 java\n")

			# Sleep before starting the next job
			fp.write(f"sleep {options.scraping_interval}\n")

if __name__ == "__main__":
	main()