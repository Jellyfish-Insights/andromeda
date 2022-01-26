import csv
import datetime
import json
import logging
import os
import re
import secrets
import sys
import time
import zipfile
from dataclasses import dataclass, field
from typing import Dict, Final, List

from logger import log, change_logger_level
from tools import UseDirectory, find_files, get_home_dir, get_project_root_path

UNZIP_DIRECTORY = 'unzipped'
DATE_REGEX = r"[0-9]{4}-[0-9]{2}-[0-9]{2}"
ZIP_FILE_REGEX = re.compile(
	rf'(?i)^(.+) ({DATE_REGEX})_({DATE_REGEX}) (?P<video_name>.+)\.zip$'
)
DATE_COLUMN_REGEX = re.compile(r"(?i)date")
EXTRACTED_DATA = "extracted_data"
DEBUG = None

@dataclass
class CSV_Data:
	filename: str
	associated_metadata: Dict
	data: List[Dict] = field(default_factory=list)

	def __post_init__(self):
		"""
		File containing data will NOT be deleted after extraction.
		"""
		provisory_data: List[Dict] = []
		try:
			with open(self.filename, "r") as csvfile:
				reader = csv.DictReader(csvfile)
				for row in reader:
					provisory_data.append(row)
		except Exception as exc:
			log.critical(f"Error parsing file '{self.filename}' !")
			log.critical(exc)
			raise

		# Cast data into the correct format for being received at C# code
		for row in provisory_data:
			self.data.append(dict())
			self.data[-1]["VideoId"] = self.associated_metadata["videoId"]
			self.data[-1]["ChannelId"] = self.associated_metadata["channelId"]
			for col in row.keys():
				if DATE_COLUMN_REGEX.search(col):
					self.data[-1]["DateMeasure"] = round(datetime.datetime.fromisoformat(row[col]).timestamp())
				else:
					self.data[-1]["Metric"] = col
					self.data[-1]["Value"] = float(row[col])

def retrieve_data_from_csv_files(
		video_name: str = None,
		associated_metadata: dict = None) -> List[CSV_Data]:
	"""
	This will remove the files we don't want to process and rename the ones
	we want to with their respective video name.
	Processed files are removed right away.
	"""
	log.debug(f"We will now process video '{video_name}'")
	csv_data_list = []
	# Confusingly, the files that come with the "_Total" prefix are the ones
	# containing daily data, and the ones suffixed with "_Table_data" contain
	# the grand totals
	with UseDirectory(UNZIP_DIRECTORY):
		extracted_files = find_files(r'\.csv$')
		for filename in extracted_files:
			if re.search(r'Table data\.csv$', filename):
				os.unlink(filename)
				continue
			csv_data_obj = CSV_Data(filename, associated_metadata)
			csv_data_list.append(csv_data_obj)
			os.unlink(filename)
	return csv_data_list

def clean_downloads():
	"""
	To be used before scraper downloads files.
	"""
	downloads = os.path.join(get_home_dir(), "Downloads")
	with UseDirectory(downloads):
		for f in find_files(ZIP_FILE_REGEX):
			os.unlink(f)

def clean_working_directory():
	if os.path.isdir(UNZIP_DIRECTORY):
		log.info("Cleaning work directory")
		for f in find_files("", UNZIP_DIRECTORY):
			full_path = os.path.join(UNZIP_DIRECTORY, f)
			os.unlink(full_path)
		os.rmdir(UNZIP_DIRECTORY)

def retrieve_data_from_zip_files() -> List[Dict]:
	log.info("Retrieving data from zip files")
	zip_files = find_files(ZIP_FILE_REGEX)
	log.info(f"Found files {zip_files}")
	os.mkdir(UNZIP_DIRECTORY)
	csv_data_list: List[CSV_Data] = []
	for f in zip_files:
		with zipfile.ZipFile(f, 'r') as zf:
			zf.extractall(UNZIP_DIRECTORY)
		match = ZIP_FILE_REGEX.search(f)
		if match is None:
			raise ValueError("Unable to extract filename from zip file!")
		named_groups = {**match.groupdict()}

		metadata_filename = f.replace("zip", "json")
		with open(metadata_filename, "r") as fp:
			metadata = json.load(fp)

		csv_data_list.extend(retrieve_data_from_csv_files(
			**named_groups,
			associated_metadata=metadata
		))

	os.rmdir(UNZIP_DIRECTORY)
	return [x.data for x in csv_data_list]

def set_debug_mode():
	global DEBUG
	debug_flags: Final = ["debug", "test", "d", "t"]
	DEBUG = any([
		bool(re.search(rf"^-{{0,2}}{flag}$", arg))
		for flag in debug_flags
		for arg in sys.argv
	])
	if DEBUG:
		log.info("DEBUG mode enabled")

def to_json(data: List[Dict]) -> None:
	data_dir = os.path.join(get_project_root_path(), EXTRACTED_DATA)
	timestamp = str(int(time.time() * 1000))
	random_hex = secrets.token_hex(4)
	with UseDirectory(data_dir):
		output_file = f"youtube_studio_{timestamp}_{random_hex}.json"
		rows, cols = len(data), len(data[0])
		log.info(f"Writing extracted data ({rows} rows, {cols} columns) to '{output_file}'")
		with open(output_file, "w") as fp:
			json.dump(data, fp)

def process_csv_data():
	if DEBUG:
		directory = os.path.join(get_project_root_path(), "tests", "data")
	else:
		directory = os.path.join(get_home_dir(), "Downloads")

	with UseDirectory(directory):
		clean_working_directory()
		csv_data_list = retrieve_data_from_zip_files()

	for csv_data in csv_data_list:
		to_json(csv_data)

def main():
	set_debug_mode()
	process_csv_data()

if __name__ == "__main__":
	main()
