/*
 * Single message handler.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { IMessageHandler } from "./imessagehandler";
import { WebSocketServer } from "../app/websocketserver";
import { MeasurementInfo, BulkMeasurementInfo } from "../models/measurement";
import { toCamelCase } from "../app/util";

export class MeasurementHandler implements IMessageHandler {
    public constructor(private readonly wss: WebSocketServer, private readonly topic: string) {
    }

    public async handle(topic: string, msg: string) {
        const measurement: MeasurementInfo = JSON.parse(msg, toCamelCase);
        const bulk = new BulkMeasurementInfo();

        if (measurement == null) {
            return;
        }

        bulk.sensorId = measurement.sensorId;
        bulk.measurements = [measurement.measurement];

        this.wss.process(bulk);
    }

    public getTopic(): string {
        return this.topic;
    }
}
