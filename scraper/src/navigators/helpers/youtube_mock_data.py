import json
import os
import secrets
import random
import time
import sys
from dataclasses import dataclass
from typing import List

MAX_ROWS_PER_FILE = 5000
AVG_VIDEOS_PER_CHANNEL = 100
MIN_ROWS_PER_VIDEO = 100
MAX_ROWS_PER_VIDEO = 500
METRICS = ["Views", "Impressions", "Subscribers gained", "Likes", "Shares"]
MIN_VALUE = 0
MAX_VALUE = 10000
# Creation time up to 10 years ago
MAX_SECONDS_AGO = round(60 * 60 * 24 * 365.25 * 10)
DATA_DIR = "generated_data"

@dataclass
class Video:
	videoId: str
	channelId: str
	createdTime: int

@dataclass
class Metric:
	eventDate: int
	metric: str
	value: int

def ceil(x):
	if x == int(x):
		return int(x)
	return int(x + 1)

def dump_to_file(rows: List):
	if not os.path.isdir(DATA_DIR):
		os.mkdir(DATA_DIR)
	filename = f"{DATA_DIR}/{secrets.token_hex(16)}.json"
	with open(filename, "w") as fp:
		json.dump(rows, fp)
	rows.clear()


def mock_data(n_videos: int):
	channels = [
		secrets.token_hex(8)
		for _ in range(ceil(n_videos / AVG_VIDEOS_PER_CHANNEL))
	]
	videos = [
		Video(
			videoId=secrets.token_hex(16),
			channelId=random.choice(channels),
			createdTime=int(time.time()) - random.randint(0, MAX_SECONDS_AGO)
			)
		for _ in range(n_videos)
	]

	rows = []
	row_count = 0
	for v in videos:
		n_rows = random.randint(MIN_ROWS_PER_VIDEO, MAX_ROWS_PER_VIDEO)
		for _ in range(n_rows):
			metric = Metric(
				eventDate=random.randint(v.createdTime,int(time.time())),
				metric=random.choice(METRICS),
				value=random.randint(MIN_VALUE, MAX_VALUE)
			)
			row = {**vars(v), **vars(metric)}
			del row["createdTime"]
			rows.append(row)
			row_count += 1
			if row_count % MAX_ROWS_PER_FILE == 0:
				dump_to_file(rows)
	if rows:
		dump_to_file(rows)
	print(f"Generated {len(channels)} channels, {len(videos)} videos, {row_count} rows.")

def main():
	if len(sys.argv) != 2:
		print(f"Usage: {__file__} <NUMBER_OF_VIDEOS>")
		return
	mock_data(int(sys.argv[1]))

if __name__ == "__main__":
	main()
