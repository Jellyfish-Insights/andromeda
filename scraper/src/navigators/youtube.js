let selector = "yta-key-metric-card";
let text = "SEE MORE";

window.findTextNode = function(selector, text)
{
	let ret = null;
	Array.from(document.querySelectorAll(selector)).every(elem =>
	{
		if (elem.innerHTML.toUpperCase() === text
				&& elem.innerText.toUpperCase() === text)
		{
			ret = elem;
			return false;
		}
		return true;
	});
	return ret;
}
findTextNode(selector, text);