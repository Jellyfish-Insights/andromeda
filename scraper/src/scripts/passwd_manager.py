from navigators.helpers.password import SymmetricEncryption

INTRO_TEXT = """
This is a utility for encrypting and decrypting passwords symmetrically. It adds \
a layer of security but an attacker possessing both the encrypted password and \
the file containing the key will be able to obtain your password. Please make \
sure you understand the implications before using this tool.
"""

def encrypt():
	message = input("What is the text you want to encrypt? ")
	se = SymmetricEncryption()
	cipher = se.encrypt(message)
	print("Encrypted text is:")
	print(f"<<{cipher}>>")
	print("Please disconsider surrounding angular brackets << >>")

def decrypt():
	cipher = input("What is the text you want to decrypt? ")
	se = SymmetricEncryption()
	try:
		message = se.decrypt(cipher)
	except ValueError:
		return
	print("Decrypted text is:")
	print(f"<<{message}>>")
	print("Please disconsider surrounding angular brackets << >>")

def main():
	print(INTRO_TEXT)
	option = None
	encrypt_options = {"e", "E"}
	decrypt_options = {"d", "D"}
	valid_options = encrypt_options | decrypt_options
	first_pass = True
	while option not in valid_options:
		if not first_pass:
			print("Invalid option.")
		first_pass = False
		option = input("Do you want to [e]ncrypt or [d]ecrypt? Please type [e] or [d]. ")

	if option in encrypt_options:
		encrypt()
	else:
		decrypt()
	

if __name__ == "__main__":
	main()