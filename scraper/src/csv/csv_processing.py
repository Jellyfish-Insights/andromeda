import os
import re
import subprocess
import traceback
import datetime
from dataclasses import dataclass, field
import pandas as pd
from typing import List, Set

from pandas.core.frame import DataFrame

UNZIP_DIRECTORY = 'unzipped'
DEBUG = True

@dataclass
class CSV_Data:
	filename: str
	video_name: str
	date_start: datetime.date
	date_end: datetime.date
	filter_by: str
	df: pd.DataFrame

def find_files(regex: str = None, join_path: str = None) -> Set[str]:
	try:
		script_path = os.path.dirname(os.path.realpath(__file__))
	except NameError:
		script_path = os.getcwd()

	if regex is None:
		regex = ""
	
	if join_path is not None:
		path = os.path.join(script_path, join_path)
	else:
		path = script_path
	
	compiled = re.compile(regex, flags=re.I)
	custom_filter = lambda filename: bool(compiled.search(filename))
	files = set(filter(
		custom_filter,
		[f for f in os.listdir(path)]
	))
	return files

def move_csv_files():
	os.chdir(UNZIP_DIRECTORY)
	new_csv_files = find_files()
	for filename in new_csv_files:
		current_dir = os.path.dirname(os.path.realpath(filename))
		parent_dir = os.path.dirname(current_dir)
		os.rename(filename, os.path.join(parent_dir, filename))
	os.chdir("..")

def clean_filename(filename: str):
	characters_to_avoid = "\"!#$&'()*;<=>?[\\]^`{|}~- "
	new_filename = filename
	for c in characters_to_avoid:
		new_filename = new_filename.replace(c, "_")
	return new_filename

def retrieve_data_from_csv_files(
		filter_by: str = None,
		date_start: datetime.date = None,
		date_end: datetime.date = None,
		video_name: str = None) -> List[CSV_Data]:
	"""
	This will remove the files we don't want to process and rename the ones
	we want to with their respective video name
	"""
	print(video_name)
	csv_data_list = []
	prefix = f"{video_name}___{filter_by}___{str(date_start)}___{str(date_end)}"
	# Confusingly, the files that come with the "_Total" prefix are the ones
	# containing daily data, and the ones suffixed with "_Table_data" contain
	# the grand totals
	os.chdir(UNZIP_DIRECTORY)
	extracted_files = find_files(r'\.csv$')
	for filename in extracted_files:
		if re.search(r'Table data\.csv$', filename):
			os.unlink(filename)
			continue
		new_name = clean_filename(f"{prefix}___{filename}")
		os.rename(filename, new_name)
		print(new_name)
		df = parse_data_from_file(new_name, video_name)
		csv_data_obj = CSV_Data(filename, video_name, date_start, date_end, filter_by, df)
		csv_data_list.append(csv_data_obj)
	os.chdir("..")
	return csv_data_list

def check_call(cmd: str) -> None:
	try:
		if DEBUG:
			subprocess.check_call(cmd, shell=True)
		else:
			subprocess.check_call(cmd, shell=True, 
					stderr=subprocess.DEVNULL, stdout=subprocess.DEVNULL)
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

def clean_working_directory():
	csv_files = find_files(r'\.csv$')
	for f in csv_files:
		check_call(f"rm '{f}'")
	check_call(f"if [ -d '{UNZIP_DIRECTORY}' ] ; then rm -r '{UNZIP_DIRECTORY}' ; fi")

def retrieve_data_from_zip_files() -> DataFrame:
	zip_files = find_files(r'\.zip$')
	check_call(f"mkdir {UNZIP_DIRECTORY}/")
	video_name_regex = re.compile(r'^(?P<filter_by>.+) (?P<date_start>[0-9]{4}-[0-9]{2}-[0-9]{2})_(?P<date_end>[0-9]{4}-[0-9]{2}-[0-9]{2}) (?P<video_name>.+)\.zip$', flags=re.I)
	csv_data_list: List[DataFrame] = []
	for f in zip_files:
		check_call(f"unzip '{f}' -d '{UNZIP_DIRECTORY}'/")
		match = video_name_regex.search(f)
		if match is None:
			raise ValueError("Unable to extract filename from zip file!")
		named_groups = {**match.groupdict()}

		try:
			named_groups["date_start"] = \
				datetime.date.fromisoformat(named_groups["date_start"])
			named_groups["date_end"] = \
				datetime.date.fromisoformat(named_groups["date_end"])
		except ValueError:
			raise ValueError("Date has inappropriate format!")

		csv_data_list.extend(retrieve_data_from_csv_files(**named_groups))
		move_csv_files()

	os.rmdir(UNZIP_DIRECTORY)
	return pd.concat([x.df for x in csv_data_list])

def parse_data_from_file(filename: str, video_name: str) -> DataFrame:
	try:
		df = pd.read_csv(filename)
	except pd.errors.ParserError:
		print(f"Error parsing file '{filename}' !")
		raise

	if "Date" in df.columns:
		df["Date"] = pd.to_datetime(df["Date"])
		df["Title"] = video_name

	return df

def main():
	clean_working_directory()
	big_table = retrieve_data_from_zip_files()
	print(big_table)
	big_table.to_csv('youtube_analytics.csv')
	return big_table

if __name__ == "__main__":
	main()