/*
 * Application entry point.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

import { MqttClient } from "../clients/mqttclient";
import { parseSettings, Settings } from "../models/settings";
import * as mongodb from "./mongodb";
import { WebSocketServer } from "./websocketserver";
import { Express } from "express";
import { connect } from "./postgresql";

// ReSharper disable once UnusedLocalImport
import express from "express";
import cors from "cors";
import "./jsonmodule";
import { MessageHandler } from "../handlers/messagehandler";
import { MeasurementHandler } from "../handlers/measurementhandler";
import { ControlMessageHandler } from "../handlers/controlmessagehandler";

export class Application {
    private readonly client: MqttClient;
    public static config: Settings;
    private readonly wss: WebSocketServer;

    public constructor() {
        Application.config = parseSettings();

        this.client = new MqttClient(Application.config.mqtt.host, Application.config.mqtt.port, Application.config.mqtt.topicShare);
        // ReSharper disable once TsResolvedFromInaccessibleModule
        const app: Express = express();
        const pool = connect(Application.config);

        app.use(cors());
        this.wss = new WebSocketServer(app, pool, Application.config.web.timeout, Application.config.web.secret, this.client);
    }

    public run() {
        mongodb.connect(Application.config.mongoDB.connectionString);

        this.client.addHandler(new MessageHandler(this.wss, Application.config.mqtt.bulkMessageTopic));
        this.client.addHandler(new MeasurementHandler(this.wss, Application.config.mqtt.bulkMeasurementTopic));
        this.client.addHandler(new ControlMessageHandler(this.wss, Application.config.mqtt.bulkControlMessageTopic));

        this.client.connect(Application.config.mqtt.username, Application.config.mqtt.password).then(() => {
            console.log("Connected to MQTT!");
            return this.client.subscribe(Application.config.mqtt.bulkMeasurementTopic);
        }).then(() => {
            console.log(`Subscribed to: ${Application.config.mqtt.bulkMeasurementTopic}`);
            return this.client.subscribe(Application.config.mqtt.bulkMessageTopic);
        }).then(() => {
            console.log(`Subscribed to: ${Application.config.mqtt.bulkMessageTopic}`);
            return this.client.subscribe(Application.config.mqtt.bulkControlMessageTopic);
        }).then(() => {
            console.log(`Subscribed to: ${Application.config.mqtt.bulkControlMessageTopic}`);
        }).catch(err => {
            console.warn(`Unable to connect to MQTT: ${err.toString()}`);
        });

        this.wss.listen(Application.config.web.port);
    }
}

export function main() {
    console.log(`Starting LiveDataHandler`);
    const app = new Application();
    app.run();
}
