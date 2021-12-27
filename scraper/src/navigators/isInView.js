window.isInView = function(elem)
{
	const rect = elem.getBoundingClientRect();
	const innerHeight = window.innerHeight;
	const innerWidth = window.innerWidth;
	return (
		rect.height > 0 && rect.width > 0 
		&& (
			rect.top > 0 && rect.top < innerHeight
			|| rect.bottom > 0 && rect.bottom < innerHeight
		)
		&& (
			rect.left > 0 && rect.left < innerWidth
			|| rect.right > 0 && rect.right < innerWidth
		)
	);
}

const elem =  document.evaluate( "(/html/body//*[not(self::script)])[1]", document, null, XPathResult.ANY_TYPE, null ) .iterateNext()  ;
return isInView(elem);
