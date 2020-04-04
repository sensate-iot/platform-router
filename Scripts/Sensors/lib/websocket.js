/*
 * Websocket client.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

const socket = require('ws');
const generate = require('./generate');
const NanoTimer = require('nanotimer');

function publish(ws, args) {
	const measurement = generate.generateMeasurement(args);

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
	timer = new NanoTimer();
	const secret = args.config.webSocket.secret;

	if(args.messages) {
		const host = args.host + ':' + args.port + '/ingress/v1/messages';
		console.log(host);
		const ws = new socket(host);

		ws.on('open', () => {
			console.log('Websocket connected!');
			timer.setInterval(publishMessage, [ws, args], args.interval.toString() + 'u');
		});
	} else {
		const host = args.host + ':' + args.port + '/ingress/v1/measurement';

		console.log(host);
		const ws = new socket(host);

		ws.on('open', () => {
			console.log('Websocket connected!');
			timer.setInterval(publish, [ws, args], args.interval.toString() + 'u');
		});
	}

}
