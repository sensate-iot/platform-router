/*
 * Maintainer parsing: figure out the file maintainer of a
 * source file.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

const fs = require('fs');

function matchArray(data, regex) {
	var match = regex.exec(data);
	var matches = [];

	while(match != null) {
		matches.push(match[1]);
		match = regex.exec(data);
	}

	if(matches.length == 0)
		return null;

	return matches;
}

function findName(data) {
	var pattern = new RegExp(/@author\ +([^\n]+)/g);
	return matchArray(data, pattern);
}

function findEmail(data) {
	var pattern = new RegExp(/@email\ +([^\n].+)/);

	if(pattern.test(data))
		return data.match(pattern)[1];

	return null;
}

function findMaintainer(data) {
	var pattern = new RegExp(/@maintainer([^\n]+)/);
	return matchArray(data, pattern);
}

function searchFile(data, filename) {
	var names = findName(data);
	const email = findEmail(data);
	const maintainers = findMaintainer(data);

	if(maintainers != null)
		names = maintainers;

	if(names == null && maintainers == null) {
		/* maintainer not found */
		console.log('--------------------------');
		console.log('No maintainer found for ' + filename);
		console.log('--------------------------');
		console.log('');

		return;
	}

	console.log('--------------------------');
	console.log('Maintainer for ' + filename + ':');
	console.log('');

	if(names.length > 1) {
		names.forEach(function(name) {
			console.log(name);
		});
	} else if(names.length == 1) {
		if(email != null) {
			console.log(names[0] + ' <' + email + '>');
		} else {
			console.log(names[0]);
		}
	}
	
	console.log('--------------------------');
	console.log('');
}

module.exports.run = function(files) {
	files.forEach(element => {
		fs.readFile(element, 'utf8', (err, data) => {
			if(err) {
				console.log('Unable to read file: ' + err);
				return;
			}

			searchFile(data, element);
		});
	});
}
