import tarfile
import os
import shutil

from logger import log
from scripts.job_finder import find_all_jobs, go_to_jobs

class BackupCurrentJobs:
	backup_jobs_dir = "bkp_jobs"

	def __enter__(self):
		self.old_path = os.getcwd()
		go_to_jobs(create=True)
		os.chdir(os.path.pardir)
		shutil.copytree("jobs", self.backup_jobs_dir)

	def __exit__(self, exc_type, exc_value, traceback):
		go_to_jobs(create=False)
		os.chdir(os.path.pardir)
		shutil.rmtree("jobs")
		os.rename(self.backup_jobs_dir, "jobs")
		os.chdir(self.old_path)

def main():
	log.info("Starting test...")
	with BackupCurrentJobs():
		os.chdir(os.path.dirname(os.path.realpath(__file__)))
		jobs_examples = tarfile.open("./data/jobs_examples.tar.bz2")
		jobs_examples.extractall("../jobs/")
		jobs_examples.close()
		all_jobs = frozenset(find_all_jobs())

	files_to_check = frozenset([
		'./user_example/profilefaker/00_profile_faker.env',
		'./user_example/youtube/01_youtube_passwd_encrypted.env',
		'./user_example/tiktok/02_tiktok_1.env',
		'./user_example/tiktok/03_tiktok_2.env',
		'./user_example/tiktok/04_tiktok_3.env',
		'./jobs_examples/user_8d6126a4-90c1-4355-bd15-14dc19955f75/tiktok/09_good_dict_good_dir.env'
	])
	if all_jobs == files_to_check:
		log.info("Test passed!")
	else:
		log.critical("Test failed.")
		if len(all_jobs - files_to_check) == 0:
			log.critical("Not all jobs were identified correctly.")
		elif len(files_to_check - all_jobs) == 0:
			log.critical("Program marked more jobs as valid than it should have.")
		else:
			log.critical("There are jobs that shouldn't be there and jobs that are there and shouldn't be.")
		log.critical(f"Output was: {all_jobs}")
		log.critical(f"Expected: {files_to_check}")

if __name__ == "__main__":
	main()