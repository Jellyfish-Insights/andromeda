import datetime
import logging
import math
import os
import re
import sys
import time
import zipfile
from dataclasses import dataclass
from typing import Final, List

import pandas as pd

from logger import log, change_logger_level
from tools import UseDirectory, find_files, get_home_dir

UNZIP_DIRECTORY = 'unzipped'
DATE_REGEX = r"[0-9]{4}-[0-9]{2}-[0-9]{2}"
ZIP_FILE_REGEX = re.compile(
	rf'(?i)^(?P<filter_by>.+) (?P<date_start>{DATE_REGEX})_(?P<date_end>{DATE_REGEX}) (?P<video_name>.+)\.zip$'
)
DATE_COLUMN_REGEX = re.compile(r"(?i)date")
INTEGER_COLUMNS_REGEX = [
	re.compile(rf"(?i){string}")
	for string in ["views", "subscribers", "impressions", "likes"]
]
EXTRACTED_DATA = "extracted_data"
DEBUG = None

@dataclass
class CSV_Data:
	filename: str
	video_name: str
	date_start: datetime.date
	date_end: datetime.date
	filter_by: str
	df: pd.DataFrame = None
	identifier: str = None

	def __post_init__(self):
		prefix = f"{self.video_name}___{self.filter_by}___{str(self.date_start)}___{str(self.date_end)}"
		try:
			filename_no_extension = re.search(r"(?i)^(.+)\.csv$", self.filename)[1]
		except IndexError:
			log.warning("Filename received is not of CSV extension")
			filename_no_extension = self.filename
		self.identifier = clean_filename(f"{prefix}___{filename_no_extension}")
		self.df = self.fetch_data()

	def fetch_data(self):
		"""
		File containing data will NOT be deleted after extraction.
		"""
		log.debug(f"Processing file {self.identifier}")
		try:
			df = pd.read_csv(self.filename)
		except pd.errors.ParserError:
			log.critical(f"Error parsing file '{self.filename}' !")
			raise

		# Cast data into the correct format
		for col in df.columns:
			if DATE_COLUMN_REGEX.search(col):
				df.rename({col: "Date"})
				df["Date"] = pd.to_datetime(df[col])
			else:
				for int_col_regex in INTEGER_COLUMNS_REGEX:
					if int_col_regex.search(col):
						for i in df.index:
							df.loc[i, col] = math.floor(df.loc[i, col])
						df[col] = df[col].astype(pd.Int64Dtype())

		df["Identifier"] = self.identifier
		df["Video Title"] = self.video_name

		return df

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
			csv_data_obj = CSV_Data(filename, video_name, date_start, date_end, filter_by)
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

def retrieve_data_from_zip_files() -> pd.DataFrame:
	log.info("Retrieving data from zip files")
	zip_files = find_files(ZIP_FILE_REGEX)
	log.info(f"Found files {zip_files}")
	os.mkdir(UNZIP_DIRECTORY)
	csv_data_list: List[pd.DataFrame] = []
	for f in zip_files:
		with zipfile.ZipFile(f, 'r') as zf:
			zf.extractall(UNZIP_DIRECTORY)
		match = ZIP_FILE_REGEX.search(f)
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

	os.rmdir(UNZIP_DIRECTORY)
	return pd.concat(
		[x.df for x in csv_data_list],
	).sort_values(by=["Date"]).reset_index(drop=True)

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

def get_output_filename():
	if not os.path.isdir(EXTRACTED_DATA):
		os.mkdir(EXTRACTED_DATA)
	timestamp = str(int(time.time() * 1000))
	return os.path.join(EXTRACTED_DATA, f"youtube_analytics_{timestamp}.csv")

def process_csv_data():
	if DEBUG:
		directory = os.path.join("tests", "data")
	else:
		directory = os.path.join(get_home_dir(), "Downloads")
	
	with UseDirectory(directory):
		clean_working_directory()
		big_table = retrieve_data_from_zip_files()

	if DEBUG:
		log.debug(big_table)

	output_file = get_output_filename()
	rows, cols = big_table.shape
	log.info(f"Writing extracted data ({rows} rows, {cols} columns) to '{output_file}'")
	big_table.to_csv(output_file)
	return big_table

def main():
	set_debug_mode()
	if DEBUG:
		change_logger_level(logging.DEBUG)
	else:
		change_logger_level(logging.DEBUG)
	process_csv_data()

if __name__ == "__main__":
	main()