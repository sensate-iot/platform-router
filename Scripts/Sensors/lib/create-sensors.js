/*
 * Tooling to mass generate sensors.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

 'use strict';

const axios = require('axios');

async function generateSensor(idx, key) {
	const name = `Test Sensor ${idx + 1}`;
	const description = `Automagically generated testing sensor #${idx + 1}`;

	return axios.post(`https://api.staging.sensateiot.com/network/v1/sensors?key=${key}`, {
		name: name,
		description: description
	}).then(value => {
		const response = {
			sensor: value.data.internalId,
			secret: value.data.secret
		};

		return response;
	}).catch(e => {
		console.log("Unable to create sensor: ");
		console.log(e);
	});
}

async function generateSensors(count, key) {
	const sensors = new Array;

	console.log("Generating sensors");
	for(let idx = 0; idx < count; idx++) {
		const sensor = await generateSensor(idx, key);
		sensors.push(sensor);

		if(idx % 100 === 0) {
			console.log("Generated 100 sensors");
		}
	}

	console.log(JSON.stringify(sensors));
}

module.exports = {
	generateSensors
}
