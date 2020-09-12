/*
 * User settings parser.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

const fs = require('fs');

function parse(path) {
	if(path == undefined)
		return undefined;

	const json = fs.readFileSync(path);

	return JSON.parse(json);
}

module.exports = {parse}
