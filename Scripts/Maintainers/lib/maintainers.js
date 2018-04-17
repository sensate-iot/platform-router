/*
 * Maintainer parsing: figure out the file maintainer of a
 * source file.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

const fs = require('fs');

function findName(data) {
	var pattern = new RegExp(/@author.([^\n]+)/);

	if(pattern.test(data))
		return data.match(pattern)[1];

	return null;
}

function findEmail(data) {
	var pattern = new RegExp(/@email\ +([^\n].+)/);

	if(pattern.test(data))
		return data.match(pattern)[1];

	return null;
}

function findMaintainer(data) {
	var pattern = new RegExp(/@maintainer([^\n]+)/);

	if(pattern.test(data))
		return data.match(pattern)[1];

	return null;
}

function searchFile(data, filename) {
	var name = findName(data);
	const email = findEmail(data);
	const maintainer = findMaintainer(data);

	if(maintainer != null)
		name = maintainer;

	if(name == null && maintainer == null) {
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
	console.log(name + ' <' + email + '>');
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
