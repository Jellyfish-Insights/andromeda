import datetime
import time
import os
import json
from sqlalchemy import Column, String, Integer, BigInteger, DateTime, select, and_
from sqlalchemy.dialects.postgresql import JSONB

from logger import log
from db import DBException, session, base, SCHEMA_NAME

class ReelInfo(base):
	__tablename__ = "reels_info"
	__table_args__ = {'schema': SCHEMA_NAME}

	# Metadata
	internal_id = Column(BigInteger, primary_key=True)
	account_name = Column(String, nullable=False)
	saved_time = Column(DateTime, nullable=False, default=datetime.datetime.now)

	# The JSON payload contains everything you might require
	# The rest of the options are just a destructuring of the JSON object,
	# for quit fetching and debugging
	json_payload = Column(JSONB, nullable=False)
	
	# Columns read from the JSON. Since they are always computed from column
	# "json_payload", we could use Postgres's Generated Columns
	# https://www.postgresql.org/docs/12/ddl-generated-columns.html
	# However, I don't know if SQL Alchemy has support for that

	reel_id = Column(String, nullable=False)
	comment_count = Column(Integer, nullable=False)
	like_count = Column(Integer, nullable=False)
	view_count = Column(Integer, nullable=False)
	play_count = Column(Integer, nullable=False)
	create_time = Column(DateTime, nullable=False)

	@staticmethod
	def add(account_name: str, item: dict, save_in_fs: bool = True):
		reel_info = ReelInfo(
			# Metadata
			account_name = account_name,

			# JSON payload
			json_payload = item,

			# Destructured data
			reel_id = int(item["id"]),
			comment_count = int(item["comment_count"]),
			like_count = int(item["like_count"]),
			view_count = int(item["view_count"]),
			play_count = int(item["play_count"]),
			create_time = datetime.datetime.fromtimestamp(int(item["taken_at"]))
		)
		log.info(f"Saving reels_id={reel_info.reel_id} to database...")

		try:
			session.add(reel_info)
			session.commit()
		except Exception as err:
			log.error("An unknown error happened!")
			log.error(err)
			raise DBException

		if save_in_fs:
			reel_info.to_json()

	@staticmethod
	def get_all(
			account_name: str = None,
			newer_than: datetime.datetime = None
			):
		videos = session.execute(
				select(ReelInfo.json_payload)
				.where(and_(
					account_name == None or ReelInfo.account_name.ilike(account_name),
					newer_than == None or ReelInfo.saved_time >= newer_than
				))
				.order_by(ReelInfo.saved_time.desc())
			)
		
		# Return first and only column
		return [x[0] for x in videos]

	def to_json(self):
		filename = f"{self.account_name}___{self.reel_id}___{int(time.time() * 10 ** 6)}.json"
		full_path = os.path.join("extracted_data", filename)
		with open(full_path, "w") as fp:
			fp.write(json.dumps(self.json_payload))

	def __str__(self):
		return str(self.__dict__)

if __name__ == "__main__":
	videos = ReelInfo.get_all()
	for row in videos:
		print(row)