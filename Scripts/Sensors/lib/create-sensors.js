/*
 * Tooling to mass generate sensors.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

 'use strict';

const axios = require('axios');

async function generateSensor(idx) {
	const name = `Test Sensor ${idx + 1}`;
	const description = `Automagically generated testing sensor #${idx + 1}`;

	return axios.post('https://api.staging.sensateiot.com/network/v1/sensors?key=fpaAFwvMQA3IeL_RT8uWSMSnrEOFIX0d', {
	//return axios.post('http://localhost:5003/network/v1/sensors?key=KTF0dTwb13mde1TTnXwBrKa4LzTKoM9m', {
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

async function generateSensors(count) {
	const sensors = new Array;

	console.log("Generating sensors");
	for(let idx = 0; idx < count; idx++) {
		const sensor = await generateSensor(idx);
		sensors.push(sensor);

		if(idx % 10 === 0) {
			console.log("Generated 10 sensors");
		}
	}

	console.log(JSON.stringify(sensors));
}

module.exports = {
	generateSensors
}
