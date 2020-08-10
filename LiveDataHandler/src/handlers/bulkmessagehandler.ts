/*
 * Single message handler.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { IMessageHandler } from "./imessagehandler";
import { WebSocketServer } from "../app/websocketserver";
import { BulkMeasurementInfo } from "../models/measurement";
import { toCamelCase } from "../app/util";
import * as gzip from "zlib";

export class BulkMeasurementHandler implements IMessageHandler {
    public constructor(private readonly wss: WebSocketServer, private readonly topic: string) {
    }

    public async handle(topic: string, data: string) {
        const msg = await this.decode(data);
        const measurements: [BulkMeasurementInfo] = JSON.parse(msg, toCamelCase);

        if (measurements == null) {
            return;
        }

        measurements.forEach(m => {
            if (!this.wss.hasOpenSocketFor(m.sensorId.toString()))
                return;

            this.wss.process(m);
        });
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

    public getTopic(): string {
        return this.topic;
    }
}
