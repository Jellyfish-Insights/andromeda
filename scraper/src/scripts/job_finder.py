import os
import random
from typing import Dict, List, Optional, Tuple, Type
from uuid import uuid4
from dotenv.main import dotenv_values
from dataclasses import dataclass

from logger import log
from scraper_middleware import ScraperMiddleWare
from models.options import Options as ScraperOptions
from tools import PreserveDirectory, UseDirectory, go_to_project_root
@dataclass
class Job:
	filename: str
	options: Dict[str, str]
	nav_name: str

	def make_full_options(self) -> Dict[str,str]:
		return {**self.options, "navigator_name": self.nav_name}

def get_jobs_path(create: bool = False):
	with PreserveDirectory():
		go_to_project_root()
		if not os.path.isdir("jobs"):
			if not create:
				raise ValueError("'jobs' directory does not exist!")
			os.mkdir("jobs")
		os.chdir("jobs")
		return os.getcwd()

def go_to_jobs(create: bool = False):
	os.chdir(get_jobs_path(create))

def create_example_jobs_directory():
	number_of_users = 5
	min_number_of_jobs = 2
	max_number_of_jobs = 6
	social_media = ["adwords", "facebook", "instagram", "tiktok", "youtube"]
	
	with UseDirectory(get_jobs_path(create=True)):
		for _ in range(number_of_users):
			new_dir = f"user_{str(uuid4())}"
			os.mkdir(new_dir)
			with UseDirectory(new_dir):
				for sm in social_media:
					os.mkdir(sm)
					with UseDirectory(sm):
						number_of_jobs = random.randint(
								min_number_of_jobs,
								max_number_of_jobs
						)
						for i in range(number_of_jobs):
							filename = "%02d_job_sample.env" % i
							open(filename, "w").close()

def get_options_and_nav_name_from_file(
			filename: str,
			discard_if_empty: bool) -> Optional[Job]:

	if discard_if_empty:
		minimum_file_size = 5
		file_size = os.stat(filename).st_size
		if file_size < minimum_file_size:
			log.debug(f"File '{filename}' has under {minimum_file_size} bytes, skipping")
			return

		options = dotenv_values(filename)

		if not options or all(value is None for value in options.values()):
			log.debug(f"Bad format for .env file in '{filename}', skipping")
			return
	else:
		options = dotenv_values(filename)

	# Check if a scraper exists with the directory name
	dirname = os.path.basename(os.path.dirname(filename))
	nav_class: Optional[Type] = ScraperMiddleWare.match_navigator_name(dirname)
	if nav_class is None:
		return

	# Let's try to create this object and see if it will complain
	nav_name = nav_class.__name__
	job = Job(filename, options, nav_name)
	full_options = job.make_full_options()
	try:
		_ = ScraperOptions(**full_options)
	except ValueError as err:
		log.debug(f"'{filename}' file does not contain plausible "
			"options for available scrapers, skipping")
		log.debug(err)
		return

	return job

def find_all_jobs(discard_empty_jobs=True) -> List[Job]:
	with UseDirectory(get_jobs_path(create=False)):
		log.info(f"Looking for .env files containing jobs, at {os.getcwd()}")
		jobs_found = []
		for dirname, dirs, files in os.walk('.'):
			for file in files:
				full_filename = os.path.join(dirname, file)
				log.debug(f"Analysing file '{full_filename}'")
				if not file.endswith(".env"):
					log.debug(f"File '{full_filename}' does not have the .env extension, skipping")
					continue

				job = get_options_and_nav_name_from_file(
					full_filename, discard_if_empty=discard_empty_jobs)

				if job is None:
					continue

				log.info(f"Found a valid job at '{full_filename}'")
				jobs_found.append(job)

	sort_lambda = lambda x: os.path.basename(x.filename)
	return sorted(jobs_found, key=sort_lambda)

if __name__ == "__main__":
	print(find_all_jobs())