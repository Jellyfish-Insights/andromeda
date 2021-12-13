import os
from sqlalchemy import create_engine
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker

class AlreadyExistent(Exception):
	pass

class Inexistent(Exception):
	pass

class DBError(Exception):
	pass

base = declarative_base()
db_string = os.environ.get('db_conn_string') or 'postgresql://brab:brickabode@localhost:5432'
db = create_engine(db_string)

Session = sessionmaker(db)
session = Session()