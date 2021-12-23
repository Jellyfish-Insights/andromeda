import re, datetime, time
from sqlalchemy import Column, String, BigInteger, DateTime, select
from sqlalchemy.exc import IntegrityError, PendingRollbackError
from sqlalchemy.orm.exc import NoResultFound

from db import base, session, Inexistent

class AccountName(base):
	__tablename__ = "account_name"
	id = Column(BigInteger, primary_key=True)
	account_name = Column(String, nullable=False, unique=True)
	updated_time = Column(DateTime, nullable=False, default=datetime.datetime.now)

	@staticmethod
	def get_current():
		try:
			account_name = session.execute(
					select(AccountName)
					.order_by(AccountName.updated_time.desc())
					.limit(1)
				).scalar_one()
			return account_name.account_name
		except NoResultFound:
			raise Inexistent

	@staticmethod
	def get_all():
		try:
			result = session.execute(
					select(AccountName)
					.order_by(AccountName.updated_time.desc())
				).scalars()
			return result
		except NoResultFound:
			raise Inexistent

	@staticmethod
	def add(account_name):
		AccountName.test(account_name)

		account_name_row = AccountName(account_name=account_name)
		try:
			session.add(account_name_row)
			session.commit()
		except (IntegrityError, PendingRollbackError):
			# Tried to add account name which already existed
			pass

		# Wait for changes to be committed to database, avoid race conditions
		time.sleep(0.25)

	@staticmethod
	def test(account_name):
		# Nowhere does it really say the maximum size is 32, but I guess
		# it is reasonable to expect it
		if not re.search(r'^@[a-zA-Z0-9_\.]{1,32}$', account_name):
			raise ValueError