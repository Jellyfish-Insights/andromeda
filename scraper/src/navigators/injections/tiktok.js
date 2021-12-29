window.DEBUG_MODE = false;
window.SEGMENT_SIZE = 15;
window.printerr = function(...args)
{
	if (DEBUG_MODE)
		console.log(...args);
}

window.printSegment = function(idx, s, stack)
{
	if (!DEBUG_MODE)
		return;
	let	start = Math.max(0, idx - SEGMENT_SIZE);
	let end = Math.min(s.length, idx + SEGMENT_SIZE);
	let segment = s.slice(start, end);
	console.log(idx, s[idx], segment, stack);
}

window.getSubstringsInCurlyBraces = function(s)
{
	const stack = [];
	const substrings = [];

	let insideDoubleQuote = false;
	let insideSingleQuote = false;
	for (let [index, c] of Object.entries(s)) {
		let idx = parseInt(index);
		if (!insideSingleQuote && c === '"' && (idx === 0 || s[idx - 1] != "\\"))
			insideDoubleQuote = !insideDoubleQuote;
		else if (!insideDoubleQuote && c === "'" && (idx === 0 || s[idx - 1] != "\\"))
			insideSingleQuote = !insideSingleQuote;
		else if (insideDoubleQuote || insideSingleQuote) {
			continue;
		}

		else if (c === "{") {
			stack.push(idx);
			printSegment(idx, s, stack);
		}
		else if (c === "}") {
			const start = stack.pop();
			printSegment(idx, s, stack);
			const end = idx + 1;
			
			if (start === undefined) {
				printerr("Malformed string");
				printerr(s.slice(0, end));
				continue;
			}
			if (stack.length !== 0)
				continue;

			const newSubstring = s.slice(start, end);
			printerr(`Inserting s[${start}:${end}] = <<< ${newSubstring} >>>`);
			substrings.push(newSubstring);
		}
	}
	return substrings;
}
window.getLongestStringInList = function(list)
{
	if (list.length === 0)
		throw "List is empty";
	
	return list.sort(
		function (a, b) {
			return b.length - a.length;
		}
	)[0];
}
window.getJsonObjectCandidates = function()
{
	const objectCandidates = [];

	document.querySelectorAll("script").forEach(elem => {
		/* Get substrings that look like a JSON-encoded object */
		const innerHTML = elem.innerHTML;
		const jsonLikes = getSubstringsInCurlyBraces(innerHTML);
		jsonLikes.forEach(jsonLike => {
			printerr(`We found a JSON-like string with ${jsonLike.length} bytes.`)
			/* Try to see if that is really a JSON object */
			try {
				const jsonObject = JSON.parse(jsonLike);
				objectCandidates.push(jsonObject);
			} catch(e) {
				if (e instanceof SyntaxError) {
					printerr("This looked like a JSON, but wasn't.");
					printerr(e);
					printerr(jsonLike)
				}
			}
		});
	});
	return objectCandidates;
}
window.validateDataStructure = function(dict)
{
	const mustContain = [
		"id",
		"desc",
		"createTime",
		"scheduleTime",
		"stats",
		"author",
		"music",
		"challenges",
		"authorStats",
	];
	const statsMustContain = [
		"diggCount",
		"shareCount",
		"commentCount",
		"playCount"
	];
	const authorStatsMustContain = [
		"followerCount",
		"followingCount",
		"heart",
		"heartCount",
		"videoCount",
		"diggCount"
	];
	return (
		mustContain.every(el => el in dict)
		&& statsMustContain.every(el => el in dict.stats)
		&& authorStatsMustContain.every(el => el in dict.authorStats)
	);
}
window.findSuitableNodes = function(obj, dataList = null)
{
	/* Finds which nodes are identified by a particular key, and returns these
	 * nodes as a list */
	
	const videoIdExample = "7046482712043244847";
	const videoIdRegex = RegExp(`[0-9]{${videoIdExample.length}}`);

	if (dataList === null)
	dataList = [];

	Object.keys(obj).forEach(key => {
		/* Checks if object is of dictionary type */
		if (obj[key].constructor != Object)
			printerr(`We've reached a leaf node: ${obj[key]}`);
		else if (key.match(videoIdRegex) && validateDataStructure(obj[key]))
			dataList.push(obj[key]);
		else
			findSuitableNodes(obj[key], dataList);			
	});

	return dataList;
}
window.getTikTokDataFromScriptTags = function()
{
	const data = [];
	const objectCandidates = getJsonObjectCandidates();
	printerr(`These are the object candidates: ${objectCandidates}`)
	objectCandidates.forEach(obj => {
		findSuitableNodes(obj).forEach(node => data.push(node));
	});
	return data;
}

return getTikTokDataFromScriptTags();