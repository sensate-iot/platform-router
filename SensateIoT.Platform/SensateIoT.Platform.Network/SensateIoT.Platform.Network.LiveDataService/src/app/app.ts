/*
 * Application entry point.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

import { MqttClient } from "../clients/mqttclient";
import { Settings } from "../models/settings";
import settings from "../../settings/appsettings.json";
import * as mongodb from "./mongodb";
import { WebSocketServer } from "./websocketserver";
import { Express } from "express";
import { connect } from "./postgresql";

// ReSharper disable once UnusedLocalImport
import express from "express";
import cors from "cors";
import "./jsonmodule";
import { MeasurementHandler } from "../handlers/messagehandler";
import { BulkMeasurementHandler } from "../handlers/bulkmessagehandler";

class Application {
    private readonly client: MqttClient;
    private readonly config: Settings;
    private readonly wss: WebSocketServer;

    public constructor() {
        const tmp = JSON.stringify(settings);
        this.config = JSON.parse(tmp);

        this.client = new MqttClient(this.config.mqtt.host, this.config.mqtt.port, this.config.mqtt.topicShare);
        // ReSharper disable once TsResolvedFromInaccessibleModule
        const app: Express = express();
        const pool = connect(this.config);

        app.use(cors());
        this.wss = new WebSocketServer(app, pool, this.config.web.timeout, this.config.web.secret);
    }

    public run() {
        mongodb.connect(this.config.mongoDB.connectionString);

        this.client.addHandler(new MeasurementHandler(this.wss, this.config.mqtt.internalMeasurementTopic));
        this.client.addHandler(new BulkMeasurementHandler(this.wss, this.config.mqtt.internalBulkMeasurementTopic));

        this.client.connect(this.config.mqtt.username, this.config.mqtt.password).then(() => {
            console.log("Connected to MQTT!");
            return this.client.subscribe(this.config.mqtt.internalBulkMeasurementTopic);
        }).then(() => {
            console.log(`Subscribed to: ${this.config.mqtt.internalBulkMeasurementTopic}`);
            return this.client.subscribe(this.config.mqtt.internalMeasurementTopic);
        }).then(() => {
            console.log(`Subscribed to: ${this.config.mqtt.internalMeasurementTopic}`);
        }).catch(err => {
            console.warn(`Unable to connect to MQTT: ${err.toString()}`);
        });

        this.wss.listen(this.config.web.port);
    }
}

export function main() {
    console.log(`Starting LiveDataHandler`);
    const app = new Application();
    app.run();
}
