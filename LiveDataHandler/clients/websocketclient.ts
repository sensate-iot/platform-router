/*
 * WebSocket client implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { Sensor, SensorModel } from "../models/sensor";
import { Types } from "mongoose";
import {createHash} from "crypto";
import * as WebSocket from "ws";
import { ISensorAuthRequest } from "../models/sensorauthrequest";
import { BulkMeasurementInfo } from "../models/measurement";
import { IWebSocketRequest } from "../models/request";

// ReSharper disable once UnusedLocalImport
import moment from "moment";

export class WebSocketClient {
    private readonly sensors: Map<string, SensorModel>;
    private readonly socket: WebSocket;

    private static timeout = 250;

    public constructor(socket: WebSocket) {
        this.socket = socket;
        this.sensors = new Map<string, SensorModel>();
        this.socket.onmessage = this.onMessage.bind(this);
    }

    private async onMessage(data: WebSocket.MessageEvent) {
        const auth = JSON.parse(data.data.toString()) as IWebSocketRequest<any>;

        switch (auth.request) {
            case "subscribe":
                await this.subscribe(auth);
                break;

            case "unsubscribe":
                break;

            default:
                console.log(`Invalid request: ${auth.request}`);
                break;
        }
    }

    private async subscribe(req: IWebSocketRequest<ISensorAuthRequest>) {
        const auth = req.data;

        console.log(`Auth request: ${auth.sensorId} with ${auth.sensorSecret} at ${auth.timestamp}`);

        // ReSharper disable once TsResolvedFromInaccessibleModule
        const date = moment(auth.timestamp).utc(true);
        date.add(WebSocketClient.timeout, "ms");

        // ReSharper disable once TsResolvedFromInaccessibleModule
        if (date.isBefore(moment().utc())) {
            this.socket.close();
            console.log(`Authorization request to late (ID: ${auth.sensorId})`);
        }

        const hash = auth.sensorSecret;
        const sensor = await Sensor.findById(new Types.ObjectId(auth.sensorId));
        auth.sensorSecret = sensor.Secret;

        const computed = createHash("sha256").update(JSON.stringify(auth)).digest("hex");

        if (computed !== hash) {
            this.socket.close();
            return;
        }

        this.sensors.set(auth.sensorId, sensor);
        console.log(`Sensor {${auth.sensorId}}{${sensor.Owner}} authorized!`);
    }

    public isServicing(id: string) {
        return this.sensors.has(id);
    }

    public compareSocket(other: WebSocket) {
        return other === this.socket;
    }

    public process(measurements: BulkMeasurementInfo) {
        const sensor = this.sensors.get(measurements.CreatedBy.toString());

        if (sensor === null) {
            return;
        }

        const data = JSON.stringify(measurements);
        this.socket.send(data);
    }
}
