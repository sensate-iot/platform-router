/*
 * Application entry point.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

'use strict';

var mqtt = require('mqtt');
var NanoTimer = require('nanotimer');

function getRandNumber(min, max) {
	return Math.floor(Math.random() * ( max - min)) + min;
}

function generateMeasurement(args) {
	let idx = 0;

	if(args.allsensors) {
		idx = getRandNumber(0, args.sensors.length);
	}

	const measurement = {
		Longitude: 2.13613511,
		Latitude: 31.215135211,
		CreatedById: args.sensors[idx].sensor,
		CreatedBySecret: args.sensors[idx].secret,
		Data: [
			{ Name: 'x', Value: Math.random() * 10 },
			{ Name: 'y', Value: Math.random() * 100 },
			{ Name: 'z', Value: Math.random() * 20 }
		]
	}

	return measurement;
}

function publish(client, args) {
	const measurement = generateMeasurement(args, args);
	client.publish('sensate/measurements', JSON.stringify(measurement));
}

function publishBulk(client, args) {
	let ary = [];
	const max = getRandNumber(args.bulk, args.bulk + 20);

	for(let idx = 0; idx < max; idx++) {
		ary.push(generateMeasurement(args, args));
	}

	client.publish('sensate/measurements/bulk', JSON.stringify(ary));
}

let timer = undefined;

module.exports.run = function (args) {
	var opts = {};
	timer = new NanoTimer();

	if(args.username != undefined) {
		opts.username = args.username;
		opts.password = args.password;
	}

	if(args.port != undefined) {
		opts.port = args.port;
	}

	var client = mqtt.connect('mqtt://'+args.host, opts);
	client.on('connect', () => {
		console.log('Connected to MQTT broker!');
	});

	if(isNaN(args.bulk))
		timer.setInterval(publish, [client, args], args.interval.toString() + 'u');
	else
		setInterval(publishBulk, args.interval, client, args);
}
