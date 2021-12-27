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

	@staticmethod
	def xpath(

				tag: str = "*",
				text: str = None,
				text_exact: bool = True,
				case_insensitive: bool = True,
				visible: bool = True,
				attributes: dict = None,
				contains_classes: list = None,
				id: str = None,
				n_th: str = None

			) -> str:
		"""
		Remember that in xpath, indices start with 1, not 0. However, to make it
		easier to communicate with Python, n_th = 0 will correspond to the first
		element returned, we will do the conversion inside this function.
		"""
		
		filters = []

		if visible:
			filters.append(XPath.visible())

		if attributes is not None:
			filters.append(XPath.attributes(attributes))

		if contains_classes is not None:
			filters.append(XPath.contains_classes(contains_classes))

		if id is not None:
			filters.append(XPath.id(id))

		if text is not None:
			if text_exact:
				filters.append(XPath.text_exact(text, case_insensitive))
			else:
				filters.append(XPath.text_contains(text, case_insensitive))

		if len(filters) == 0:
			use_filters = ""
		else:
			use_filters = f"[{' and '.join(filters)}]"

		if n_th is not None:
			n_th_filter = f"[{n_th + 1}]"
		else:
			n_th_filter = ""

		return f"(/html/body//{tag}{use_filters}){n_th_filter}"