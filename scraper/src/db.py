import os
import json
import re

from dataclasses import dataclass
from typing import Union
from urllib.parse import quote

from sqlalchemy import create_engine
from sqlalchemy.schema import CreateSchema
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker

from tools import get_project_root_path, UseDirectory
from logger import log

SETTINGS_FILE = "appsettings.json"
KEY_FOR_SETTINGS_FILE = "ConnectionStrings"
KEY_INSIDE_KEY = "DataLakeDatabase"

DEFAULT_VENDOR = "postgresql"
DEFAULT_DATABASE = "postgres"
DEFAULT_UNAME = "postgres"
DEFAULT_PASSWD = "root"
DEFAULT_HOST = "localhost"
DEFAULT_PORT = 5432

PYTHON_CONN_STR_REGEX = re.compile(
	r'^(?P<vendor>[^:]+)://'
	r'(?P<uname>[^:]+):'
	r'(?P<passwd>[^@]+)@'
	r'(?P<host>[^:/]+)'
	r'(:(?P<port>[0-9]+))?'				# optional
	r'(/(?P<database>.+)|/?)$'			# optional
)

CS_CONN_STR_HOST_REGEX = re.compile(r'Host=([^;]+)')
CS_CONN_STR_DATABASE_REGEX = re.compile(r'Database=([^;]+)')
CS_CONN_STR_UNAME_REGEX = re.compile(r'Username=([^;]+)')
CS_CONN_STR_PASSWD_REGEX = re.compile(r'Password=([^;]+)')
CS_CONN_STR_PORT_REGEX = re.compile(r'Port=([0-9]+)')

SCHEMA_NAME = "general_scraper_v1"

class DBException(Exception):
	pass

class AlreadyExistent(DBException):
	pass

class Inexistent(DBException):
	pass

@dataclass
class ConnStr:
	vendor: str
	host: str
	database: str
	uname: str
	passwd: str
	port: Union[int,str]

	@staticmethod
	def from_csharp(s: str) -> 'ConnStr':
		match_host = CS_CONN_STR_HOST_REGEX.search(s)
		match_database = CS_CONN_STR_DATABASE_REGEX.search(s)
		match_uname = CS_CONN_STR_UNAME_REGEX.search(s)
		match_passwd = CS_CONN_STR_PASSWD_REGEX.search(s)
		match_port = CS_CONN_STR_PORT_REGEX.search(s)

		if not all([match_host, match_database, match_uname, match_passwd, match_port]):
			raise ValueError("This is not a C# connection string")

		log.debug(f"C# string doesn't supply a SQL vendor, assuming '{DEFAULT_VENDOR}'")
		vendor = DEFAULT_VENDOR
		host = match_host[1]
		database = match_database[1]
		uname = match_uname[1]
		passwd = match_passwd[1]
		port = int(match_port[1])
		return ConnStr(vendor, host, database, uname, passwd, port)

	@staticmethod
	def from_python(s: str) -> 'ConnStr':
		match = PYTHON_CONN_STR_REGEX.search(s)
		if not match:
			raise ValueError("This is not a RFC-1738-style connection string")

		destructured = {**match.groupdict()}

		# Default for postgres is to log in to database named as user, if no
		# database is supplied
		if destructured.get("database") is None:
			destructured["database"] = destructured["uname"]
		if destructured.get("port") is None:
			destructured["port"] = 5432
		
		return ConnStr(**destructured)

	@staticmethod
	def from_unknown_string(s: str) -> 'ConnStr':
		try:
			conn_str = ConnStr.from_python(s)
		except ValueError:
			try:
				conn_str = ConnStr.from_csharp(s)
			except ValueError:
				raise ValueError("Connection string invalid")

		return conn_str

	@staticmethod
	def default_conn_str() -> 'ConnStr':
		return ConnStr(DEFAULT_VENDOR, DEFAULT_HOST, DEFAULT_DATABASE, DEFAULT_UNAME,
			DEFAULT_PASSWD, DEFAULT_PORT)

	@staticmethod
	def test() -> bool:
		python_strings = [
			'postgres://user:password@localhost:5432/database',
			'postgres://user:password@localhost:5432/',
			'postgres://user:password@localhost:5432',
			'postgres://user:password@localhost',
			'postgres://user:password@localhost/',
			'postgres://user:password@localhost/database',
			'postgres://user:password@localhost:',		# error
			'postgres://:password@localhost/database'	# error
			'postgres://user:password@:5432',			# error
		]
		for ps in python_strings:
			try:
				conn_str_to_python = ConnStr.from_python(ps).to_python()
			except ValueError:
				conn_str_to_python = "Error!"
			log.info(f"The input of '{ps}' was parsed as '{conn_str_to_python}'")

		cs_strings = [
			"Host=analytics_platform;Database=analytics_platform;Username=fee;Password=dbpassword;Port=5432",
			"Host=analytics_platform;Database=analytics_platform;Username=fee;Port=5432;Password=dbpassword",
			"Password=dbpassword;Host=analytics_platform;Database=analytics_platform;Username=fee;Port=5432",
		]
		for cs in cs_strings:
			try:
				conn_str_to_python = ConnStr.from_csharp(cs).to_python()
			except ValueError:
				conn_str_to_python = "Error!"
			log.info(f"The input of '{cs}' was parsed as '{conn_str_to_python}'")

	############################################################################
	# INSTANCE METHOD
	############################################################################
	def encode(self) -> 'ConnStr':
		"""Returns a copy of the object, changing every field to be RFC-3986
		compliant (i.e. escape reserved characters)
		"""
		copy = ConnStr(self.vendor, self.host, self.database, self.uname,
			self.passwd, self.port)
		for key, value in vars(copy).items():
			if (quoted := quote(str(value))) != value:
				setattr(copy, key, quoted)
		return copy

	def to_python(self):
		encoded = self.encode()
		return f"{encoded.vendor}://{encoded.uname}:{encoded.passwd}@{encoded.host}:{encoded.port}/{encoded.database}"

def parse_db_string() -> str:
	jobs_dir = os.path.join(get_project_root_path(), "jobs")
	try:
		with UseDirectory(jobs_dir):
			with open(SETTINGS_FILE, "r") as fp:
				data = json.load(fp)

			conn_strings = data[KEY_FOR_SETTINGS_FILE]
			if type(conn_strings) != dict or len(conn_strings) == 0:
				raise KeyError

			if len(conn_strings) > 1:
				s = conn_strings[KEY_INSIDE_KEY]
			else:
				s = conn_strings[0]
			return ConnStr.from_unknown_string(s).to_python()

	except (FileNotFoundError, KeyError, ValueError) as e:
		log.debug(f"Could not read {SETTINGS_FILE}, using default connection string.")
		log.debug(f"Exception raised = {e}")
		return ConnStr.default_conn_str().to_python()

base = declarative_base()
db_string = parse_db_string()
log.info(f"Connecting to database with string '{db_string}'")
db = create_engine(db_string)

if not db.dialect.has_schema(db, SCHEMA_NAME):
	db.execute(CreateSchema(SCHEMA_NAME))

Session = sessionmaker(db)
session = Session()

def setup_db():
	import models.account_name, models.video_info
	base.metadata.create_all(db)
