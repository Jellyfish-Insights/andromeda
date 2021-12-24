#!/usr/bin/env python3
from dataclasses import dataclass
import os
import re
import subprocess
import argparse
import random
import sys
import signal
from typing import List

from db import setup_db
from logger import log
from models.options import Options as ScraperOptions
from scripts.job_finder import Job, find_all_jobs, get_jobs_path
from tools import UseDirectory, get_project_root_path

SCHEDULER_SHELL_SCRIPT = "scheduler.sh"
DEFAULT_SLEEP_INTERVAL = 60

BASH_PROLOGUE = f"""\
#!/usr/bin/env bash

log() {{
	echo -e "[$(date +"%Y-%m-%d %H:%M:%S")] [BASH-SCHEDULER] $1"
}}

control_c() {{
    log "Interrupt received... exiting... :("
    exit
}}
trap control_c SIGINT SIGTERM SIGHUP

return_total=0
cd '{get_project_root_path()}'

"""

BASH_GET_RETURN_VALUE = """
return_value=$?
log "Return value was $return_value"
return_total=$((return_total + return_value))
"""

BASH_EPILOGUE = """
if [ "$return_total" -ne 0 ] ; then
    log "Finished running scheduler, but some jobs failed."
else
    log "All jobs concluded successfully."
fi

exit $return_total
"""

DEBUG = False

@dataclass
class SchedulerOptions:
	random_order: bool
	sleep_interval: int
	dry_run: bool

def write(*args, **kwargs):
	if DEBUG:
		print(*args, **kwargs)
	else:
		with open(SCHEDULER_SHELL_SCRIPT, "w") as fp:
			sole_argument = args[0] + '\n'
			fp.write(sole_argument)

def write_append(*args, **kwargs):
	if DEBUG:
		print(*args, **kwargs)
	else:
		with open(SCHEDULER_SHELL_SCRIPT, "a") as fp:
			sole_argument = args[0] + '\n'
			fp.write(sole_argument)

def parse() -> SchedulerOptions:
	parser = argparse.ArgumentParser(
		description="Schedules jobs for scraping social media. Reads from .env "
		'files placed in the "jobs" directory'
	)
	parser.add_argument(
		'--random_order',
		'-r',
		action='store_true',
		help="By default, jobs are run in alphabetical order. If this is set, "
		"a random order is used instead."
	)
	parser.add_argument(
		'--dry_run',
		'-d',
		action='store_true',
		help="Quit after producing file with instructions. Don't run jobs."
	)
	parser.add_argument(
		'--sleep_interval',
		'-i',
		action='store',
		type=int,
		default=DEFAULT_SLEEP_INTERVAL,
		help="Defines how long the program should sleep between every invocation "
			"of the jobs (in seconds)."
	)
	args = parser.parse_args()
	print(args)
	return SchedulerOptions(**vars(args))

def start_scheduler_shell_script():
	write(BASH_PROLOGUE)

def add_scheduler_shell_script(
			index: int,
			job: Job,
			sleep_interval: int) -> None:

	full_options = job.make_full_options()
	options = ScraperOptions(**full_options)

	basename = os.path.basename(job.filename)
	cmd = options.generate_cmd()
	redacted = re.sub(r"password_encrypted='(.+)'", r"password_encrypted='********'", cmd)
	redacted = re.sub(r"password_plain='(.+)'", r"password_plain='********'", redacted)
	write_append(f'log "Now executing job #{index + 1} with instructions found at {basename}"')
	write_append(f'log "Running command {redacted}"')
	write_append(cmd)
	write_append(BASH_GET_RETURN_VALUE)
	# A lot of java processes are left dangling for no reason
	# This is still an open issue by BrowserMob Proxy Py
	# https://github.com/AutomatedTester/browsermob-proxy-py/issues/8
	write_append(f'log "Cleaning and sleeping for {sleep_interval} seconds"')
	write_append("killall -9 -q java")
	# Sleep before starting the next job
	write_append(f"sleep {sleep_interval}")
	write_append("")

def show_statistics(jobs: List[Job]):
	buffer = [
		"",
		"",
		"We found the following jobs: ",
		""
	]
	for job in jobs:
		buffer.append(f"\t{os.path.basename(job.filename)}")
	buffer.append("")

	nav_names = [job.nav_name for job in jobs]
	counter = {nav_name: nav_names.count(nav_name) for nav_name in set(nav_names)}
	
	buffer.append("Jobs are distributed as: ")
	buffer.append("")
	for nav_name, frequency in counter.items():
		buffer.append(f"\t{nav_name:18s}{frequency:3d}")
	buffer.append(f"\t{'_' * 21}")
	buffer.append(f"\t{'Total':18s}{sum(counter.values()):3d}")
	buffer.append("")

	log.info("\n".join(buffer))

def sig_handle(signal_received, frame):
	log.info(f"Process received signal = {signal_received}. Cleaning up and exiting.")
	os.unlink(SCHEDULER_SHELL_SCRIPT)
	sys.exit(1)

def main():
	options = parse()
	setup_db()
	
	jobs = find_all_jobs()
	if not jobs:
		log.critical("No jobs were found.")
		return
	show_statistics(jobs)

	if options.random_order:
		log.info("Shuffling jobs to a random order.")
		random.shuffle(jobs)

	with UseDirectory(get_jobs_path()):
		log.info(f"We will write instructions to '{SCHEDULER_SHELL_SCRIPT}'")
		start_scheduler_shell_script()		

		for i in range(len(jobs)):
			add_scheduler_shell_script(i, jobs[i], options.sleep_interval)
		write_append(BASH_EPILOGUE)

		if options.dry_run:
			return

		signal.signal(signal.SIGINT, sig_handle)
		signal.signal(signal.SIGTERM, sig_handle)

		full_path = os.path.realpath(SCHEDULER_SHELL_SCRIPT)
		log.info(f"Running file '{full_path}' generated with instructions.")
		sp = subprocess.run(f"bash {SCHEDULER_SHELL_SCRIPT}", shell=True)
		if sp.returncode != 0:
			log.error("One or more of your jobs failed. Please check the logs above.")
		os.unlink(SCHEDULER_SHELL_SCRIPT)

	sys.exit(sp.returncode)
if __name__ == "__main__":
	main()