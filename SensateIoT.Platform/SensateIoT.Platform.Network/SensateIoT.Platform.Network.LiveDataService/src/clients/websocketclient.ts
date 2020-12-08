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
import * as jwt from "jsonwebtoken";
import { SensorLinksClient } from "./sensorlinksclient";
import { Pool } from "pg";

// ReSharper disable once UnusedLocalImport
import moment from "moment";
import { AuditLogsClient } from "./auditlogsclient";
import { AuditLog, RequestMethod } from "../models/auditlog";

export class WebSocketClient {
    private readonly sensors: Map<string, SensorModel>;
    private readonly socket: WebSocket;
    private authorized: boolean;
    private readonly client: SensorLinksClient;
    private userId: string;

    public constructor(
        private readonly logs: AuditLogsClient,
        private readonly secret: string,
        private readonly remote: string,
        private readonly timeout: number,
        socket: WebSocket, pool: Pool
    ) {
        this.socket = socket;
        this.sensors = new Map<string, SensorModel>();
        this.socket.onmessage = this.onMessage.bind(this);
        this.authorized = false;
        this.client = new SensorLinksClient(pool);
        this.userId = "";
    }

    private async createLog() {
        const log: AuditLog = {
            timestamp: new Date(),
            authorId: this.userId,
            route: "/live/v1/measurements",
            method: RequestMethod.WebSocket,
            ipAddress: this.remote 
        };

        await this.logs.createEntry(log);
    }

    private async onMessage(data: WebSocket.MessageEvent) {
        const req = JSON.parse(data.data.toString()) as IWebSocketRequest<any>;

        if (req === null || req === undefined) {
            return;
        }

        if (req.request !== "auth" && !this.authorized) {
            console.log(`Received unauthorized request: ${req.request}`);
            return;
        }

        switch (req.request) {
            case "subscribe":
                await this.subscribe(req);
                break;

            case "unsubscribe":
                this.unsubscribe(req);
                break;

            case "auth":
                this.authorized = await this.auth(req);
                break;

            default:
                console.log(`Invalid request: ${req.request}`);
                break;
        }
    }

    private async auth(req: IWebSocketRequest<string>) {
        const result = await this.verifyRequest(req.data);
        this.createLog();
        return result;
    }

    public getUserId(): string {
        return this.userId;
    }

    private unsubscribe(req: IWebSocketRequest<ISensorAuthRequest>) {
        if (!this.sensors.has(req.data.sensorId)) {
            return;
        }

        console.log(`Removing sensor: ${req.data.sensorId}`);
        // TODO: update router.
        this.sensors.delete(req.data.sensorId);
    }

    private async subscribe(req: IWebSocketRequest<ISensorAuthRequest>) {
        const auth = req.data;

        console.log(`Auth request: ${auth.sensorId} with ${auth.sensorSecret} at ${auth.timestamp}`);

        // ReSharper disable once TsResolvedFromInaccessibleModule
        const date = moment(auth.timestamp).utc(true);
        date.add(this.timeout, "ms");

        // ReSharper disable once TsResolvedFromInaccessibleModule
        if (moment.utc().isSameOrAfter(date)) {
            this.socket.close();
            console.log(`Authorization request to late (ID: ${auth.sensorId})`);
            return;
        }

        const hash = auth.sensorSecret;
        const sensor = await Sensor.findById(new Types.ObjectId(auth.sensorId));

        if (sensor === null || sensor === undefined) {
            return;
        }

        auth.sensorSecret = sensor.Secret;

        const computed = createHash("sha256").update(JSON.stringify(auth)).digest("hex");

        if (computed !== hash) {
            const links = await this.client.getSensorLinks(auth.sensorId);
            const match = links.find((value) => {
                return value.UserId === this.userId;
            });

            if (match === null || match === undefined) {
                this.socket.close();
                return;
            }
        }

        this.sensors.set(auth.sensorId, sensor);
        console.log(`Sensor {${auth.sensorId}}{${sensor.Owner}} authorized!`);

        // TODO: subscribe to router
    }

    public isServicing(id: string) {
        return this.sensors.has(id);
    }

    public compareSocket(other: WebSocket) {
        return other === this.socket;
    }

    public process(data: string) {
        if (!this.authorized) {
            return;
        }

        this.socket.send(data);
    }

    public async authorize(token: string) {
        this.authorized = await this.verifyRequest(token);
        await this.createLog();
    }

    public isAuthorized() {
        return this.authorized;
    }

    private verifyRequest(token: string) {
        if (token === null || token === undefined) {
            return false;
        }

        return new Promise<boolean>((resolve) => {
            jwt.verify(token, this.secret, (err, obj: any) => {
                if (err) {
                    resolve(false);
                    return;
                }

                this.userId = obj.sub;
                resolve(true);
            });
        });
    }
}
