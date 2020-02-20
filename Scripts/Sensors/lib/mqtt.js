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

function generateLocationAround(lat, lng, radius) {
	const x0 = lng;
	const y0 = lat;
	const deg = radius / 111300;

	const m = Math.random();
	const n = Math.random();

	const w = deg * Math.sqrt(m);
	const t = 2 * Math.PI * n;
	const x = w * Math.cos(t);
	const y = w * Math.sin(t);

	const xp = x / Math.cos(y0);

	return [
		y + y0,
		xp + x0
	]
}

function generateMeasurement(args) {
	let idx = 0;

	if(args.allsensors) {
		idx = getRandNumber(0, args.sensors.length);
	}

	const location = generateLocationAround(51.59137, 4.7786, 800);

	const measurement = {
		Longitude: location[1],
		Latitude: location[0],
		CreatedById: args.sensors[idx].sensor,
		CreatedBySecret: args.sensors[idx].secret,
		Data: {
			x: {
				Value: Math.random() * 10,
				Unit: "m/s2"
			},
			y: {
				Value: Math.random() * 100,
				Unit: "m/s2"
			},
			z: {
				Value: Math.random() * 20,
				Unit: "m/s2"
			}
		}
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
		timer.setInterval(publishBulk, [client, args], args.interval.toString() + 'u')
}
