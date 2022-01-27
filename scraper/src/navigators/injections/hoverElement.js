window.hoverElement = function(elem)
{
	const hoverEvent = new MouseEvent('mouseover', {
		'view': window,
		'bubbles': true,
		'cancelable': true
	});
	elem.dispatchEvent(hoverEvent);
}