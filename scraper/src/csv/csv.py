import os
import re
import subprocess
import traceback
from uuid import uuid4
from typing import Callable

def has_extension(extension: str) -> Callable[[str], bool]:
	def wrap(filename):
		return bool(re.search(rf'\.{extension}', filename, flags=re.IGNORECASE))
	return wrap

def check_call(cmd: str) -> None:
	try:
		subprocess.check_call(cmd, shell=True)
	except subprocess.CalledProcessError:
		print(f"Command '{cmd}' returned an error!")
		exit(1)
	except subprocess.SubprocessError:
		print(f"Could not start the process for command '{cmd}'!")
		exit(1)
	except Exception as e:
		print(f"An unknown error happened when running '{cmd}'!")
		traceback.print_tb(e)
		exit(1)


script_path = os.path.dirname(os.path.realpath(__file__))
print(script_path)

input_files = list(filter(
	has_extension('zip'),
	[
		f
		for f in os.listdir(script_path)
	]
))

print(input_files)
check_call("mkdir unzipped/")
for f in input_files:
	check_call(f"unzip '{f}' -d unzipped/")
	# Prefix and remove whitespace
	prefix = str(uuid4())
	check_call(f'cd unzipped && for f in * ; do mv "$f" "{prefix}_$(echo "$f" | tr " " "_")" ; done')
	check_call("mv unzipped/* .")
check_call("rm -r unzipped/")