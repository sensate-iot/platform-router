#!/usr/bin/env node

'use strict';

const program = require('commander');
const wrk = require('../lib/http-generator');

function main() {
	program.version('0.0.1', '-v, --version')
	.option('-W, --wrk', 'generate WRK configuration', false)
	.option('-i --interval <num>', 'interval to generate measurements', 1000)
	.option('-d, --datapoints <num>', 'number of datapoints to generate for each measurement', 3)
	.option('-c, --count <num>', 'number of measurements to generate', 0)
	.option('-s, --sensors <sensorPath>', 'sensor secrets & ID\'s', undefined);

	program.parse(process.argv);

	if(program.count === 0 || program.count == null) {
		console.log("Unable to generate 0 messages. Please provide a measurement count using the --count flag.");
		program.outputHelp();
		return;
	}

	const args = {
		sensorData: process.cwd() + "\\" + program.sensors,
		wrk: program.wrk,
		interval: +program.interval,
		count: +program.count,
		datapoints: +program.datapoints
	}

	if(args.wrk) {
		wrk.generate(args);
	}
}

main();
