import re

def trim(text: str):
	substitutions_list = [
		("\n", " "),
		("\t", " "),
		(r"[ ]{2,}", " ")
	]
	new_text = text
	for pair in substitutions_list:
		new_text = re.sub(pair[0], pair[1], new_text)
	return new_text