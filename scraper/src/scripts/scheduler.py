#!/usr/bin/env python3
from dataclasses import dataclass
import os
import random
import subprocess
import argparse
from dotenv import dotenv_values

from db import setup_db
from logger import log
from models.options import Options

SCHEDULER_SHELL_SCRIPT = "scheduler.sh"
DEFAULT_SLEEP_INTERVAL = 60

@dataclass
class SchedulerOptions:
	random_order: bool
	sleep_interval: int

def parse() -> SchedulerOptions:
	parser = argparse.ArgumentParser(
		description="Schedules jobs for scraping social media. Reads from .env "
		'files placed in the "jobs" directory'
	)
	parser.add_argument(
		'--random_order',
		'-r',
		action='store_true',
		type=int,
		help="By default, jobs are run in alphabetical order. If this is set, "
		"a random order is used instead."
	)
	parser.add_argument(
		'--sleep_interval',
		'-i',
		action='store',
		type=int,
		default=DEFAULT_SLEEP_INTERVAL,
		help="Defines how long the program should sleep between every invokation "
			"of the jobs (in seconds)."
	)
	args = parser.parse_args()
	return SchedulerOptions(**vars(args))

def start_scheduler_shell_script():
	with open(SCHEDULER_SHELL_SCRIPT, "w") as fp:
		fp.write("#!/usr/bin/env bash\n")

def add_scheduler_shell_script(job_file):
	job_dict = dotenv_values(job_file)
	with open(SCHEDULER_SHELL_SCRIPT, "w+") as fp:
		fp.write("#!/usr/bin/env bash\n")

		# A lot of java processes are left dangling for no reason
		# This is still an open issue by BrowserMob Proxy Py
		# https://github.com/AutomatedTester/browsermob-proxy-py/issues/8
		fp.write("killall -9 java\n")
		# Sleep before starting the next job
		fp.write(f"sleep {options.scraping_interval}\n")

def main():
	options = parse()
	setup_db()
	
	script_path = os.path.dirname(os.path.abspath(__file__))
	jobs_path = os.path.join(os.path.dirname(script_path), "jobs")
	if not os.path.isdir(jobs_path):
		log.critical(f"Jobs folder '{jobs_path}' does not exist!")
		return
	jobs = filter(
		os.path.isfile, 
		[
			os.path.abspath(os.path.join(jobs_path, f))
			for f in os.listdir(jobs_path)
		]
	)
	if not jobs:
		log.critical("No jobs were found.")
	log.info("")
	log.info("We found the following jobs: \n\t" 
		+ "\n\t".join([os.path.basename(job_file) for job_file in jobs])
		+ "\n"
	)
	start_scheduler_shell_script()
	for job_file in jobs:
		add_scheduler_shell_script(job_file)
	return
	os.chdir("../credentials/")
	with open("to_scrape.sh", "w") as fp:
		fp.write("#!/usr/bin/env bash\n")

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
	log.info("hello")
	main()