/*
 * Generate WRK test data.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

const fs = require('fs');
const crypto = require('crypto');
const unirest = require('unirest');

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
	];
}

function generateMeasurement(sensors) {
	let idx = getRandNumber(0, sensors.length);

	const location = generateLocationAround(51.59137, 4.7786, 1200);

	const measurement = {
		longitude: location[1].toFixed(5),
		latitude: location[0].toFixed(5),
		createdById: sensors[idx].sensorId,
		createdBySecret: sensors[idx].sensorSecret,
		data: {
			x: {
				value: (Math.random() * 10).toFixed(5),
				unit: "m/s2",
				precision: 0.01,
				accuracy:  0.5
			},
			y: {
				value: (Math.random() * 100).toFixed(5),
				unit: "m/s2",
				precision: 0.01,
				accuracy:  0.5
			},
			z: {
				value: (Math.random() * 200).toFixed(5),
				unit: "m/s2",
				precision: 0.01,
				accuracy:  0.5
			}
		}
	}

	const hash = crypto.createHash('sha256').update(JSON.stringify(measurement));
	measurement.createdBySecret = `$${hash.digest('hex')}==`;

	return measurement;
}

function send(content) {
	var req = unirest("POST", "http://localhost:8080/v1/processor/measurements");

	req.headers({
		"Cache-Control": "no-cache",
		"Accept": "*/*",
		"Connection": "keep-alive",
		"Content-Type": "application/json"
	});

	req.type("json");
	req.send(JSON.stringify(content));

	req.end(function (res) {
		if (res.error) {
			throw new Error(res.error);
		}

		//console.log(res.body);
	});
}

function generateSend(args, sensors) {
	const measurements = [];

	for(let idx = 0; idx < args.count; idx++) {
		measurements.push((generateMeasurement(sensors)));
	}

	send(measurements);
}

function generate(args) {
	const result = fs.readFileSync(args.sensorData, "utf8");
	const obj = JSON.parse(result);

	setInterval(function() {
		generateSend(args, obj);
	}, args.interval);
}

module.exports = {
	generate
}
