/*
 * Application entry point.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

import { MqttClient } from "./mqtt/mqttclient";
import { Settings } from "./models/settings";
import settings from "../settings/appsettings.json";
import * as mongodb from "./mongodb";
import { MeasurementCollection, MeasurementInfo } from "./models/measurement";
import { WebSocketServer } from "./websocketserver";
import "./jsonmodule";

// ReSharper disable once UnusedLocalImport
import express from "express";

class Application {
    private readonly client: MqttClient;
    private readonly config: Settings;
    private readonly wss: WebSocketServer;

    constructor() {
        const tmp = JSON.stringify(settings);
        this.config = JSON.parse(tmp);

        this.client = new MqttClient(this.config.mqtt.host, this.config.mqtt.port);
        // ReSharper disable once TsResolvedFromInaccessibleModule
        this.wss = new WebSocketServer(express(), this.config.web.secret);
    }

    public onMessage(topic: string, msg: string) {
        if (topic === this.config.mqtt.internalBulkMeasurementTopic) {
            const measurements: MeasurementCollection = JSON.parse(msg);

            if (measurements == null)
                return;

            measurements.Measurements.forEach(m => { this.wss.onMeasurementReceived(measurements.CreatedBy, m); });
        } else if (topic === this.config.mqtt.internalMeasurementTopic) {
            const measurement: MeasurementInfo = JSON.parse(msg);

            if (measurement == null)
                return;

            this.wss.onMeasurementReceived(measurement.CreatedBy, measurement.Measurement);
        }
    }

    public run() {
        mongodb.connect(this.config.mongoDB.connectionString);

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

        this.client.setHandleMessage((topic, msg) => {
            this.onMessage(topic, msg);
        });

        this.wss.listen(this.config.web.port);
    }
}

const app = new Application();
app.run();
export default app;
