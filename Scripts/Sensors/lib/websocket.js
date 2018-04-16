/*
 * Websocket client.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

const socket = require('ws');

function publish(ws, args) {
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

	ws.send(JSON.stringify(measurement));
}

module.exports.run = function (args) {
	const host = args.host + ':' + args.port + '/measurement';
	const ws = new socket('ws://' + host);

	ws.on('open', () => {
		console.log('Websocket connected!');
		setInterval(publish, args.interval, ws, args);
	});
}
