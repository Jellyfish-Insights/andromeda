import os
from sqlalchemy import create_engine
from sqlalchemy.schema import CreateSchema
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker

SCHEMA_NAME = "tiktok_scraper_v1"

class AlreadyExistent(Exception):
	pass

class Inexistent(Exception):
	pass

class DBError(Exception):
	pass

base = declarative_base()
db_string = os.environ.get('db_conn_string') or 'postgresql://brab:brickabode@localhost:5432'
db = create_engine(db_string)

if not db.dialect.has_schema(db, SCHEMA_NAME):
	db.execute(CreateSchema(SCHEMA_NAME))

Session = sessionmaker(db)
session = Session()

def setup_db():
	import models.account_name, models.video_info
	base.metadata.create_all(db)