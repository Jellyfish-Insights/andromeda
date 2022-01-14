#!/usr/bin/env python3
from dataclasses import dataclass
import os
import re
import argparse
import random
from typing import List, Optional

from logger import log
from models.options import Options as ScraperOptions
from scripts.job_finder import Job, find_all_jobs, get_jobs_path
from tools import UseDirectory, get_project_root_path

SCHEDULER_SHELL_SCRIPT = "scheduler.sh"
DEFAULT_SLEEP_INTERVAL = 60

BASH_PROLOGUE = f"""\
#!/usr/bin/env bash

log() {{
	echo -e "[$(date +"%Y-%m-%d %H:%M:%S")] [BASH-SCHEDULER] $1" <tee>
}}

control_c() {{
    log "Interrupt received... exiting... :("
    exit
}}
trap control_c SIGINT SIGTERM SIGHUP

cd '{get_project_root_path()}'

"""

BASH_GET_RETURN_VALUE = """
return_value=$?
log "Return value was $return_value"
"""

DEBUG = False

@dataclass
class SchedulerOptions:
	random_order: bool
	sleep_interval: int
	log_file: Optional[str]

	def make_tee_string(self) -> str:
		if self.log_file is not None:
			return f'2>&1 | tee -a "{self.log_file}"'
		return ""

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
		'--sleep_interval',
		'-i',
		action='store',
		type=int,
		default=DEFAULT_SLEEP_INTERVAL,
		help="Defines how long the program should sleep between every invocation "
			"of the jobs (in seconds)."
	)
	parser.add_argument(
		'--log_file',
		'-l',
		action='store',
		type=str,
		help="Defines file (if any) to which bash logs should be saved."
	)
	args = parser.parse_args()
	print(args)
	return SchedulerOptions(**vars(args))

def add_scheduler_shell_script(
			index: int,
			job: Job,
			options: SchedulerOptions) -> None:

	full_options = job.make_full_options()
	scraper_options = ScraperOptions(**full_options)

	basename = os.path.basename(job.filename)
	cmd = scraper_options.generate_cmd()

	redacted = re.sub(r"password_encrypted='(.+)'", r"password_encrypted='********'", cmd)
	redacted = re.sub(r"password_plain='(.+)'", r"password_plain='********'", redacted)

	write_append(f'log "Now executing job #{index + 1} with instructions found at {basename}"')
	write_append(f'log "Running command {redacted}"')
	write_append(f"{cmd} {options.make_tee_string()}")
	write_append(BASH_GET_RETURN_VALUE)
	# A lot of java processes are left dangling for no reason
	# This is still an open issue by BrowserMob Proxy Py
	# https://github.com/AutomatedTester/browsermob-proxy-py/issues/8
	write_append(f'log "Cleaning and sleeping for {options.sleep_interval} seconds"')
	write_append("killall -9 -q java")
	# Sleep before starting the next job
	write_append(f"sleep {options.sleep_interval}")
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

def main():
	options = parse()
	
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
		
		bash_prologue_sub = BASH_PROLOGUE.replace("<tee>", options.make_tee_string())
		write(bash_prologue_sub)

		for i in range(len(jobs)):
			add_scheduler_shell_script(i, jobs[i], options)

if __name__ == "__main__":
	main()