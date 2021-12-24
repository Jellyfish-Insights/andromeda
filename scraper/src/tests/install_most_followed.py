import os, re
from db import db, base
from models.account_name import AccountName

def install_most_followed():
	# Start database
	base.metadata.create_all(db)

	os.chdir(os.path.dirname(os.path.realpath(__file__)))
	with open("most_followed.txt") as fp:
		all = fp.read()

	for account_name in [x for x in all.split("\n") if re.search("^@[a-zA-Z0-9_]+", x)]:
		AccountName.add(account_name)

if __name__ == "__main__":
	install_most_followed()