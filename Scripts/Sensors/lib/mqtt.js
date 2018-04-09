/*
 * Application entry point.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

'use strict';

var mqtt = require('mqtt');

function publish(client, args) {
	const measurement = {
		Longitude: 2.13613511,
		Latitude: 31.215135211,
		CreatedById: args.id,
		CreatedBySecret: args.secret,
		Data: [
			{ Name: 'x', Value: Math.random() * 10 },
			{ Name: 'y', Value: Math.random() * 100 },
			{ Name: 'z', Value: Math.random() * 20 }
		]
	}

	client.publish('sensate/measurements', JSON.stringify(measurement));
}

module.exports.run = function (args) {
	var opts = {};

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

	setInterval(publish, args.interval, client, args);
}
