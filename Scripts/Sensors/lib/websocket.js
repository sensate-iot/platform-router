/*
 * Websocket client.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

const fs = require('fs');
const socket = require('ws');
const generate = require('./generate');
const NanoTimer = require('nanotimer');

function publish(ws, sensors) {
	const measurement = generate.generateMeasurement(sensors);

	ws.send(JSON.stringify(measurement));
}

function publishMessage(ws, args) {
	const message = {
		SensorId: args.id,
		Secret: args.secret,
		Data: "Hello, World!"
	}

	ws.send(JSON.stringify(message));
}


var timer = undefined;

module.exports.run = function (args) {
	const raw = fs.readFileSync(args.sensorPath, "utf8");
	const sensors = JSON.parse(raw);
	timer = new NanoTimer();

	if(args.messages) {
		const host = args.host + ':' + args.port + '/ingress/v1/messages';
		const ws = new socket(host);

		ws.on('open', () => {
			console.log('Websocket connected!');
			timer.setInterval(publishMessage, [ws, args], args.interval.toString() + 'u');
		});
	} else {
		const host = args.host + ':' + args.port + '/ingress/v1/measurement';
		const ws = new socket(host);

		ws.on('open', () => {
			console.log('Websocket connected!');
			timer.setInterval(publish, [ws, sensors], args.interval.toString() + 'u');
		});
	}

}
