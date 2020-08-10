#!/usr/bin/env node

/*
 * Authorization test tool.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

'use strict';

const program = require('commander');
const http = require('../lib/http-generator');

function main() {
	program.version('0.0.1', '-v, --version')
	.option('-i --interval <num>', 'interval to generate measurements', 1000)
	.option('-C, --count <num>', 'number of measurements to generate', 0)
	.option('-H, --host <host>', 'address of the authorization server', 'localhost:8080')
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
		datapoints: +program.datapoints,
		host: program.host
	}

    http.generate(args);
}

main();
