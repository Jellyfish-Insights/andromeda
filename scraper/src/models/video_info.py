import datetime, time
from sqlalchemy import Column, String, Integer, BigInteger, DateTime, select, and_
from sqlalchemy.dialects.postgresql import JSONB

from logger import logger
from db import DBError, session, base

class VideoInfo(base):
	__tablename__ = "video_info"

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

	tiktok_id = Column(BigInteger, nullable=False)
	# "desc" is an internal PSQL word
	description = Column(String, nullable=False)
	comment_count = Column(Integer, nullable=False)
	digg_count = Column(Integer, nullable=False)
	play_count = Column(Integer, nullable=False)
	share_count = Column(Integer, nullable=False)
	create_time = Column(DateTime, nullable=False)

	@staticmethod
	def add(account_name: str, item: dict, save_in_fs: bool = False):
		vid_info = VideoInfo(
			# Metadata
			account_name = account_name,

			# JSON payload
			json_payload = item,

			# Destructured data
			tiktok_id = int(item["id"]),
			description = item["desc"],
			comment_count = item["stats"]["commentCount"],
			digg_count = item["stats"]["diggCount"],
			play_count = item["stats"]["playCount"],
			share_count = item["stats"]["shareCount"],
			create_time = datetime.datetime.fromtimestamp(item["createTime"])
		)
		logger.info(f"Saving tiktok_id={vid_info.tiktok_id} to database...")

		try:
			session.add(vid_info)
			session.commit()
		except BaseException as err:
			logger.error("An unknown error happened!")
			logger.error(err)
			raise DBError

		if save_in_fs:
			vid_info.to_json()

	@staticmethod
	def get_all(
			account_name: str = None,
			newer_than: datetime.datetime = None
			):
		videos = session.execute(
				select(VideoInfo.json_payload)
				.where(and_(
					account_name == None or VideoInfo.account_name.ilike(account_name),
					newer_than == None or VideoInfo.saved_time >= newer_than
				))
				.order_by(VideoInfo.saved_time.desc())
			)
		
		# Return first and only column
		return [x[0] for x in videos]

	def to_json(self):
		with open(f"json/{int(time.time() * 10 ** 6)}.json", "w") as fp:
			fp.write(self.json_payload)

	def __str__(self):
		return str(self.__dict__)

if __name__ == "__main__":
	videos = VideoInfo.get_all()
	for row in videos:
		print(row)