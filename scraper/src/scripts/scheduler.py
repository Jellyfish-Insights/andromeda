#!/usr/bin/env python3
from dataclasses import dataclass
import os
import re
import subprocess
import argparse
import random
import sys
from dotenv import dotenv_values

from db import setup_db
from logger import log
from models.options import Options as ScraperOptions

SCHEDULER_SHELL_SCRIPT = "scheduler.sh"
DEFAULT_SLEEP_INTERVAL = 60

BASH_PROLOGUE = """\
#!/usr/bin/env bash

log() {
	echo -e "$(date +'%%Y-%%m-%%d %%H:%%M:%%S') [BASH-SCHEDULER] $1"
}

control_c() {
    log "Interrupt received... exiting... :("
    exit
}
trap control_c SIGINT SIGTERM SIGHUP

return_total=0
cd '%(script_path)s'

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
	args = parser.parse_args()
	print(args)
	return SchedulerOptions(**vars(args))

def start_scheduler_shell_script(script_path):
	write(BASH_PROLOGUE % {'script_path': script_path})

def add_scheduler_shell_script(
			job_index: int,
			job_file: str,
			sleep_interval: int) -> None:
	job_dict = dotenv_values(job_file)
	options = ScraperOptions(**job_dict)

	basename = os.path.basename(job_file)
	cmd = options.generate_cmd()
	redacted = re.sub(r"password_encrypted='(.+)'", r"password_encrypted='********'", cmd)
	redacted = re.sub(r"password_plain='(.+)'", r"password_plain='********'", redacted)
	write_append(f'log "Now executing job #{job_index + 1} with instructions found at {basename}"')
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

def main():
	options = parse()
	setup_db()
	
	script_path = os.path.dirname(os.path.abspath(__file__))
	scraper_root_path = os.path.dirname(script_path)
	jobs_path = os.path.join(scraper_root_path, "jobs")
	log.info(f"Looking for .env files containing jobs, at {jobs_path}")
	if not os.path.isdir(jobs_path):
		log.critical(f"Jobs folder '{jobs_path}' does not exist!")
		return
	jobs = sorted(list([
		os.path.abspath(os.path.join(jobs_path, f))
		for f in os.listdir(jobs_path)
		if os.path.isfile(os.path.join(jobs_path, f)) and f.endswith(".env")
	]))
	if not jobs:
		log.critical("No jobs were found.")
		return
	log.info("")
	log.info("We found the following jobs: \n\t" 
		+ "\n\t".join([os.path.basename(job_file) for job_file in jobs])
		+ "\n"
	)

	if options.random_order:
		log.info("Shuffling jobs to a random order.")
		random.shuffle(jobs)

	log.info(f"We will write instructions to '{SCHEDULER_SHELL_SCRIPT}'")
	start_scheduler_shell_script(scraper_root_path)
	for i in range(len(jobs)):
		add_scheduler_shell_script(i, jobs[i], options.sleep_interval)
	write_append(BASH_EPILOGUE)

	log.info(f"Running file generated with instructions.")
	sp = subprocess.run(f"bash {SCHEDULER_SHELL_SCRIPT}", shell=True)
	if sp.returncode != 0:
		log.error("One or more of your jobs failed. Please check the logs above.")
	os.unlink(SCHEDULER_SHELL_SCRIPT)

	sys.exit(sp.returncode)

if __name__ == "__main__":
	main()