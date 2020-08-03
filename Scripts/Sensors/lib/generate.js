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

function generateMeasurement(sensors) {
	let idx = getRandNumber(0, sensors.length);

	const location = generateLocationAround(51.59137, 4.7786, 1200);

	const measurement = {
		longitude: +(location[1].toFixed(5)),
		latitude: +(location[0].toFixed(5)),
		sensorId: sensors[idx].sensorId,
		secret: sensors[idx].sensorSecret,
		data: {
			x: {
				value: +((Math.random() * 10).toFixed(5)),
				unit: "m/s2",
				precision: 0.01,
				accuracy:  0.5
			},
			y: {
				value: +((Math.random() * 100).toFixed(5)),
				unit: "m/s2",
				precision: 0.01,
				accuracy:  0.5
			},
			z: {
				value: +((Math.random() * 200).toFixed(5)),
				unit: "m/s2",
				precision: 0.01,
				accuracy:  0.5
			}
		}
	}

	const hash = crypto.createHash('sha256').update(JSON.stringify(measurement));
	measurement.secret = `$${hash.digest('hex')}==`;

	return measurement;
}


module.exports = {
	generateMeasurement
}
