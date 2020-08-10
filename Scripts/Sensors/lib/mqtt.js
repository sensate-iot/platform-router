/*
 * Application entry point.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

'use strict';

var mqtt = require('mqtt');
const fs = require('fs');
var NanoTimer = require('nanotimer');
const generate = require('./generate');

function publish(client, sensors) {
	const measurement = generate.generateMeasurement(sensors);
	client.publish('sensate/measurements', JSON.stringify(measurement));
}

function publishBulk(client, args, sensors) {
	let ary = [];
	const max = getRandNumber(args.bulk, args.bulk + 20);

	for(let idx = 0; idx < max; idx++) {
		ary.push(generateMeasurement(sensors));
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

	const raw = fs.readFileSync(args.sensorPath, "utf8");
	const sensors = JSON.parse(raw);

	var client = mqtt.connect('mqtt://'+args.host, opts);
	client.on('connect', () => {
		console.log('Connected to MQTT broker!');
	});

	if(isNaN(args.bulk)) {
		timer.setInterval(publish, [client, sensors], args.interval.toString() + 'u');
	} else {
		timer.setInterval(publishBulk, [client, args, sensors], args.interval.toString() + 'u')
	}
}
