import os
import random
import re
from typing import Dict, List, Union
from uuid import uuid4
from dotenv.main import dotenv_values

from logger import log
from scraper_middleware import ScraperMiddleWare
from models.options import Options as ScraperOptions

AVAILABLE_NAVIGATOR_NAMES: List[str] = ScraperMiddleWare.get_available_navigators().keys()

def go_to_project_root():
	"""Changes to the /src directory by going upwards in the directory tree and
	looking for the presence of "main.py"
	"""
	while True:
		files = [x for x in os.listdir() if os.path.isfile(x)]
		current_dir = os.getcwd()
		if os.path.basename(current_dir) == "src" and "main.py" in files:
			return
		parent_dir = os.path.dirname(current_dir)
		if parent_dir == current_dir:
			raise ValueError("The project root could not be found!")
		os.chdir(os.path.pardir)

def go_to_jobs(create: bool = False):
	go_to_project_root()
	if not os.path.isdir("jobs"):
		if not create:
			raise ValueError("'jobs' directory does not exist!")
		os.mkdir("jobs")
	os.chdir("jobs")

def create_example_jobs_directory():
	number_of_users = 5
	min_number_of_jobs = 2
	max_number_of_jobs = 6
	social_media = ["adwords", "facebook", "instagram", "tiktok", "youtube"]
	
	go_to_jobs(create=True)
	for _ in range(number_of_users):
		new_dir = f"user_{str(uuid4())}"
		os.mkdir(new_dir)
		os.chdir(new_dir)
		for sm in social_media:
			os.mkdir(sm)
			os.chdir(sm)
			number_of_jobs = random.randint(min_number_of_jobs, max_number_of_jobs)
			for i in range(number_of_jobs):
				filename = "%02d_job_sample.env" % i
				open(filename, "w").close()
			os.chdir(os.path.pardir)
		os.chdir(os.path.pardir)

def has_dict_got_plausible_options(
			env_dict: Dict[str, str],
			dirname: str) -> bool:
	nav_name = is_dir_in_available_navigators(dirname)
	if nav_name is None:
		return False

	# Let's try to create this object and see if it will complain
	kwargs = {**env_dict, "navigator_name": nav_name}
	try:
		options = ScraperOptions(**kwargs)
		return True
	except ValueError as err:
		log.debug(err)
		return False
	

def is_dir_in_available_navigators(dirname: str) -> Union[str, None]:
	for nav_name in AVAILABLE_NAVIGATOR_NAMES:
		regex = re.compile(rf"(?i)^([0-9]+_)?{nav_name}$")
		if regex.search(dirname):
			return nav_name
	log.debug(f"Directory '{dirname}' matches no known scraper.")
	return None

def find_all_jobs():
	minimum_file_size = 5

	go_to_jobs(create=False)
	jobs_found = []
	for dirname, dirs, files in os.walk('.'):
		for file in files:
			full_filename = os.path.join(dirname, file)
			if not file.endswith(".env"):
				log.debug(f"File '{full_filename}' does not have the .env extension, skipping")
				continue

			file_size = os.stat(full_filename).st_size
			if file_size < minimum_file_size:
				log.debug(f"File '{full_filename}' has under {minimum_file_size} bytes, skipping")
				continue

			env_dict = dotenv_values(full_filename)
			if not env_dict or all(value is None for value in env_dict.values()):
				log.debug(f"Bad format for .env file in '{full_filename}', skipping")
				continue

			if not has_dict_got_plausible_options(env_dict, os.path.basename(dirname)):
				log.debug(f"'{full_filename}' file does not contain plausible "
					"options for available scrapers, skipping")
				continue

			log.info(f"Found a valid job at '{full_filename}'")
			jobs_found.append(full_filename)

	return jobs_found
