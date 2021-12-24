import re

class XPath:
	@staticmethod
	def text_exact(
				text: str,
				case_insensitive: bool = True) -> str:
		if case_insensitive:
			text_to_search = text.lower()
			xpath = """
			text()
				[
					translate(
						.,
						'ABCDEFGHIJKLMNOPQRSTUVWXYZ',
						'abcdefghijklmnopqrstuvwxyz'
					)='$text_to_search'
				]
			"""
			xpath = re.sub(r'[\n\t ]+', '', xpath)
			xpath = re.sub(r'\$text_to_search', text_to_search, xpath)
			return xpath
		else:
			return f"text()='{text}'"

	@staticmethod
	def text_contains(
				text: str,
				case_insensitive: bool = True) -> str:
		if case_insensitive:
			text_to_search = text.lower()
			xpath = """
			text()
				[
					contains(
						translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),
						'$text_to_search'
					)
				]"
			"""
			xpath = re.sub(r'[\n\t ]+', '', xpath)
			xpath = re.sub(r'\$text_to_search', text_to_search, xpath)
			return xpath
		else:
			text_to_search = text
			xpath = """
			text()
				[
					contains(
						.,
						'$text_to_search'
					)
				]
			"""
			xpath = re.sub(r'[\n\t ]+', '', xpath)
			xpath = re.sub(r'\$text_to_search', text_to_search, xpath)
			return xpath

	@staticmethod
	def visible():
		"""Other filters will be added as we discover them"""
		return "not(self::script)"

	@staticmethod
	def attributes(attr_dict: dict):
		selectors = []
		for key, value in attr_dict.items():
			selectors.append(f"@{key}='{value}'")
		return " and ".join(selectors)

	@staticmethod
	def contains_classes(class_list: list):
		selectors = []
		for cl in class_list:
			selectors.append(f"contains(concat(' ', normalize-space(@class), ' '), ' {cl} ')")
		return " and ".join(selectors)

	@staticmethod
	def id(id_name):
		return f"@id='{id_name}'"