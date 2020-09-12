#!/usr/bin/env node

'use strict';

const program = require('commander');
const mqtt = require('../lib/mqtt');
const generate = require('../lib/create-sensors');
const websocket = require('../lib/websocket');
const settings = require('../lib/settings-parser');

async function main() {
	program.version('0.1.0', '-v, --version')
		.option('-g, --generate', 'generate Sensate IoT sensors', false)
		.option('-m, --mqtt', 'run the MQTT client')
		.option('-M, --messages', 'generate message\'s')
		.option('-w, --websocket', 'run the websocket client')
		.option('-s, --secret <secret>', 'sensor secret key')
		.option('-u, --user <username>', 'specifiy the MQTT broker username', undefined)
		.option('-p, --pw <password>', 'specifiy the MQTT broker password', undefined)
		.option('-H, --host <host>', 'server hostname', 'localhost')
		.option('-P, --port <port>', 'server port number', '1883')
		.option('-i, --id <sensor_id>', 'sensor id')
		.option('-b --bulk <max>', 'min amount', undefined)
		.option('-a, --allsensors', 'send measurements from multiple sensors', false)
		.option('-I, --interval <interval>', 'set the update interval', '1000')
        .option('-S, --sensors <path>', 'sensor data path', undefined)
        .option('-k, --key <key>', 'sensor generation API key', undefined)
        .option('-C, --count <key>', 'number of sensors to generate', 10)
		.option('-c, --config <config>', 'set configuration file', undefined);

	program.parse(process.argv);

	if(!program.mqtt && !program.websocket) {
		program.websocket = true;
	}

	const args = {
		generate: program.generate,
		username: program.user,
		password: program.pw,
		secret: program.secret,
		id: program.id,
		host: program.host,
		port: parseInt(program.port, 10),
		bulk: parseInt(program.bulk, 10),
		interval: parseInt(program.interval, 10),
		raw_config: program.config,
		config: settings.parse(program.config),
		allsensors: program.allsensors,
		messages: program.messages,
		sensorPath: `${process.cwd()}/${program.sensors}`
	}

	if(args.config != undefined) {
		args.secret = args.config.sensorSecret;
		args.password = args.config.mqttPassword;
		args.username = args.config.mqttUsername;
		args.id = args.config.sensorId;
		args.sensors = args.config.availableSensors;
		args.websocket = args.config.webSocket;
	}

	if(program.generate) {
        if (program.key == null) {
            console.log("Please supply a key using -k <key>");
            program.help();
        }

		await generate.generateSensors(+program.count, program.key);
	} else if(program.mqtt) {
		mqtt.run(args);
	} else {
		websocket.run(args);
	}
}

main().catch(error => {
	console.log("Unable run sensate-sensor");
});
