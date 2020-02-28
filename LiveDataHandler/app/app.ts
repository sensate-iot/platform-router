/*
 * Application entry point.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

import { MqttClient } from "../clients/mqttclient";
import { Settings } from "../models/settings";
import settings from "../settings/appsettings.json";
import * as mongodb from "./mongodb";
import { BulkMeasurementInfo, MeasurementInfo } from "../models/measurement";
import { WebSocketServer } from "./websocketserver";
import "./jsonmodule";
import * as gzip from "zlib";

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

    public async onMessage(topic: string, data: string) {
        if (topic === this.config.mqtt.internalBulkMeasurementTopic) {
            const msg = await this.decode(data);

            const measurements: [BulkMeasurementInfo] = JSON.parse(msg);

            if (measurements == null)
                return;

            measurements.forEach(m => {
                if (!this.wss.hasOpenSocketFor(m.CreatedBy.toString()))
                    return;

                this.wss.process(m);
            });
        } else if (topic === this.config.mqtt.internalMeasurementTopic) {
            const measurement: MeasurementInfo = JSON.parse(data);
            const bulk = new BulkMeasurementInfo();


            if (measurement == null) {
                return;
            }

            bulk.CreatedBy = measurement.CreatedBy;
            bulk.Measurements = [measurement.Measurement];

            this.wss.process(bulk);
        }
    }

    private decode(data: string) {
        const buf = Buffer.from(data, "base64");

        return new Promise<string>((resolve, reject) => {
            gzip.unzip(buf, (err, buffer) => {
                if (err) {
                    reject(err);
                }

                const content = buffer.toString("utf-8");
                resolve(content);
            });
        });
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

export function main() {
    const app = new Application();
    app.run();
}
