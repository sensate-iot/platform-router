/*
 * Measurement generating functions.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

var crypto = require('crypto');

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

	const location = generateLocationAround(51.59137, 4.7786, 1200);

	const measurement = {
		longitude: location[1],
		latitude: location[0],
		createdById: args.sensors[idx].sensor,
		createdBySecret: args.sensors[idx].secret,
		data: {
			x: {
				value: Math.random() * 10,
				unit: "m/s2"
			},
			y: {
				value: Math.random() * 100,
				unit: "m/s2"
			},
			z: {
				value: Math.random() * 20,
				unit: "m/s2"
			}
		}
	}

	const hash = crypto.createHash('sha256').update(JSON.stringify(measurement));
	measurement.CreatedBySecret = `$${hash.digest('hex')}==`;

	return measurement;
}

module.exports = {
	generateMeasurement
}
