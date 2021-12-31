from base64 import b64encode, b64decode
from cryptography.fernet import Fernet
from hashlib import sha256
from binascii import Error as BinAsciiError

from logger import log

class SymmetricEncryption:
	def __init__(self):
		# Change the following line as desired
		password = "my_super_secret_password___BtpdG8VBwSzGPkCiKKTDfvlXamRy7K6rfs/NGqxjOOY="

		hasher = sha256()
		hasher.update(bytes(password, encoding="utf-8"))
		bytes_encoded = hasher.digest()
		key = b64encode(bytes_encoded)

		self.f = Fernet(key)

	def encrypt(self, message: str) -> str:
		message_bytes = bytes(message, encoding="utf-8")
		cipher_bytes = self.f.encrypt(message_bytes)
		cipher = b64encode(cipher_bytes).decode("utf-8")
		return cipher

	def decrypt(self, cipher: str) -> str:
		try:
			cipher_bytes = b64decode(bytes(cipher, encoding="utf-8"))
		except BinAsciiError:
			log.critical("This is not a properly encoded message.")
			raise ValueError from BinAsciiError
		message_bytes = self.f.decrypt(cipher_bytes)
		message = message_bytes.decode("utf-8")
		return message

if __name__ == "__main__":
	text = input("What is the text you want to encrypt? ")
	se = SymmetricEncryption()
	cipher = se.encrypt(text)
	print("Encrypted text is:")
	print(f"<<{cipher}>>")