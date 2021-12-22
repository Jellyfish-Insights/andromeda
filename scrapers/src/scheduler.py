import re, sys, os, random

from arg_parser import parse
from models.account_name import AccountName
from models.video_info import VideoInfo
from db import base, db

ACCOUNT_NAME_PLACEHOLDER = "<ACCOUNT_NAME>"
SCRAPER_SCRIPT = "scraper.py"

def main():
	base.metadata.create_all(db)
	options = parse(check_account_name=False)
	if options.account_name != ACCOUNT_NAME_PLACEHOLDER:
		print(f"You need to use {ACCOUNT_NAME_PLACEHOLDER} as a placeholder for account name")
		exit(1)
	
	os.chdir(os.path.dirname(os.path.realpath(__file__)))
	with open("to_scrape.sh", "w") as fp:
		fp.write("#!/usr/bin/env bash\n")
		command = " ".join(sys.argv)

		distinct_accounts = list(AccountName.get_all())
		if len(distinct_accounts) == 0:
			print("We are not currently following any accounts. "
				"Please INSERT into \"account_name\" table to start.")
			return

		# At each pass, we will follow a random order. Hopefully, this will help
		# get data in a homogeneous way for all accounts, while not getting us
		# blocked
		random.shuffle(distinct_accounts)
		for obj in distinct_accounts:
			edited_command = re.sub(ACCOUNT_NAME_PLACEHOLDER, obj.account_name, command)
			filename = os.path.basename(__file__)
			edited_command = re.sub(filename, SCRAPER_SCRIPT, edited_command)
			fp.write(f"python3 {edited_command}\n")
			# A lot of java processes are left dangling for no reason
			fp.write("killall -9 java\n")
			fp.write(f"sleep {options.scraping_interval}\n")

if __name__ == "__main__":
	main()