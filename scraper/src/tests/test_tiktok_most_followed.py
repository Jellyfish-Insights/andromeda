import os
import re
from tools import dirname_from_file, get_project_root_path, UseDirectory

USER_DIRNAME = "test_most_followed"

def install_most_followed():
	with UseDirectory(dirname_from_file(__file__)):
		with open("tiktok_most_followed.txt") as fp:
			all_lines = fp.read()

	tiktok_most_followed = [
		x
		for x in all_lines.split("\n")
		if re.search(r"^@[a-zA-Z0-9_]+", x)
	]

	path_to_dir = [
		get_project_root_path(),
		"jobs",
		USER_DIRNAME,
		"tiktok"
	]
	path_to_dir_joined = os.path.join(*path_to_dir)
	with UseDirectory(path_to_dir_joined, create_if_nonexistent=True, create_parents=True):
		for account in tiktok_most_followed:
			with open(f"{account}.env", "w") as fp:
				fp.write(f"account_name={account}\n")


if __name__ == "__main__":
	install_most_followed()