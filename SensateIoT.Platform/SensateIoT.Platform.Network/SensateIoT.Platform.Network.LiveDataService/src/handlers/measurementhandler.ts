/*
 * Single message handler.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { IMessageHandler } from "./imessagehandler";
import { WebSocketServer } from "../app/websocketserver";
import { BulkMeasurementInfo, Measurement } from "../models/measurement";
import * as gzip from "zlib";
import { sensateiot } from "../../generated/proto";
import { ObjectId } from "mongodb";
import { ClientType } from "../models/clienttype";

export class MeasurementHandler implements IMessageHandler {
    public constructor(private readonly wss: WebSocketServer, private readonly topic: string) { }

    public async handle(topic: string, data: string) {
        const msg = await this.decode(data);
        const measurements = sensateiot.MeasurementData.decode(msg);
        const blocks = this.groupMeasurements(measurements);

        blocks.forEach(block => {
            if (!this.wss.hasOpenSocketFor(block.sensorId.toString(), ClientType.MeasurementClient)) {
                return;
            }

            this.wss.processMeasurements(block);
        });
    }

    private decode(data: string) {
        const buf = Buffer.from(data, "base64");

        return new Promise<Buffer>((resolve, reject) => {
            gzip.unzip(buf, (err, buffer) => {
                if (err) {
                    reject(err);
                }

                resolve(buffer);
            });
        });
    }

    private groupMeasurements(measurements: sensateiot.MeasurementData): BulkMeasurementInfo[] {
        const result: BulkMeasurementInfo[] = [];

        const sorted = measurements.Measurements.sort((a, b) => {
            if (a.SensorID < b.SensorID) {
                return -1;
            }

            if (a.SensorID > b.SensorID) {
                return 1;
            }

            return 0;
        });

        let dict = {};

        sorted.forEach((el) => {
            if (el.SensorID in dict) {
                dict[el.SensorID].push(el);
            } else {
                dict[el.SensorID] = [];
                dict[el.SensorID].push(el);
            }
        });

        for (let id in dict) {
            const block = new BulkMeasurementInfo();

            block.measurements = new Array<Measurement>();
            block.sensorId = ObjectId.createFromHexString(id);

            dict[id].forEach((elem: sensateiot.Measurement) => {
                const m = new Measurement();

                m.data = new Map();

                elem.Datapoints.forEach(k => {
                    m.data[k.Key] = {
                        unit: k.Unit,
                        precision: k.Precision,
                        accuracy: k.Accuracy,
                        value: k.Value
                    };
                });

                m.location = {
                    type: 'point',
                    coordinates: [elem.Longitude, elem.Latitude]
                };

                m.platformTime = new Date((elem.PlatformTime.seconds as number * 1000) + (elem.PlatformTime.nanos / 1e6));
                m.timestamp = new Date((elem.Timestamp.seconds as number * 1000) + (elem.Timestamp.nanos / 1e6));

                    
                block.measurements.push(m)
            });

            result.push(block);
        }

        return result;
    }

    public getTopic(): string {
        return this.topic;
    }
}
