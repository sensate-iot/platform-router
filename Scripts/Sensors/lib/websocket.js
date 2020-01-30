/*
 * Websocket client.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

const socket = require('ws');

function publish(ws, args) {
	const measurement = {
		Longitude: 5.13613511,
		Latitude: 55.215135211,
		CreatedById: args.id,
		CreatedBySecret: args.secret,
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

module.exports.run = function (args) {
	if(args.messages) {
		const host = args.host + ':' + args.port + '/messages';
		const ws = new socket('ws://' + host);

		ws.on('open', () => {
			console.log('Websocket connected!');
			setInterval(publishMessage, args.interval, ws, args);
		});
	} else {
		const host = args.host + ':' + args.port + '/measurement';
		const ws = new socket('ws://' + host);

		ws.on('open', () => {
			console.log('Websocket connected!');
			setInterval(publish, args.interval, ws, args);
		});
	}

}
