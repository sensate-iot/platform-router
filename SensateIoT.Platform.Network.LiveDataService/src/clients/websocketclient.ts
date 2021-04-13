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
import { ISensorAuthRequest } from "../requests/sensorauthrequest";
import { IWebSocketRequest } from "../requests/request";
import * as jwt from "jsonwebtoken";

// ReSharper disable once UnusedLocalImport
import moment from "moment";
import { AuditLogsClient } from "./auditlogsclient";
import { AuditLog, RequestMethod } from "../models/auditlog";
import { ApiKeyClient } from "./apikeyclient";
import { IApiKeyAuthenticationRequest } from "../requests/apikeyauthenticationrequest";
import { ClientType } from "../models/clienttype";
import { MqttClient } from "./mqttclient";
import { Command, stringifyCommand } from "../commands/command";
import { LiveSensorCommand } from "../commands/livesensorcommand";
import { Application } from "../app/app";
import { PingRequest } from "../requests/pingrequest";
import { Subscription } from "../models/subscription";

export class WebSocketClient {
    private readonly sensors: Map<string, Subscription>;
    private readonly socket: WebSocket;
    private authorized: boolean;
    private userId: string;

    public constructor(
        private readonly logs: AuditLogsClient,
        private readonly keys: ApiKeyClient,
        private readonly secret: string,
        private readonly remote: string,
        private readonly timeout: number,
        private readonly type: ClientType,
        private readonly mqtt: MqttClient,
        socket: WebSocket
    ) {
        this.socket = socket;
        this.sensors = new Map<string, Subscription>();
        this.socket.onmessage = this.onMessage.bind(this);
        this.authorized = false;
        this.userId = "";
    }

    public getUserId(): string {
        return this.userId;
    }
  
    public getConnectedSensors(): string[] {
        const sensorIds: string[] = [];

        this.sensors.forEach((_, key) => {
            sensorIds.push(key);
        });

        return sensorIds;
    }

    public isServicing(id: string) {
        return this.sensors.has(id);
    }

    public compareSocket(other: WebSocket) {
        return other === this.socket;
    }

    public process(data: string) {
        if (!this.authorized) {
            console.log("Unable to write live data to unauthorized client!")
            return;
        }

        console.log(`Writing data for user ${this.userId}.`);
        this.socket.send(data);
    }

    public async authorize(token: string) {
        this.authorized = await this.verifyRequest(token);
        await this.createLog();
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

    private ping(req: IWebSocketRequest<PingRequest>) {
        console.log(`Received PING request from ${this.userId}. Sending PONG.`)
        req.data.ping = "pong";
        this.socket.send(JSON.stringify(req.data));
    }

    private unsubscribe(req: IWebSocketRequest<ISensorAuthRequest>) {
        if (!this.sensors.has(req.data.sensorId)) {
            return;
        }

        console.log(`Unsubscribing sensor ${req.data.sensorId}`);
        const subscription = this.sensors.get(req.data.sensorId);

        if (subscription === undefined) {
            console.error(`Unexpected empty subscription found for sensor ${req.data.sensorId}.`)
            return;
        }

        subscription.remove();

        if (!subscription.empty()) {
            return;
        }

        console.log(`Removing sensor: ${req.data.sensorId}`);
        this.sensors.delete(req.data.sensorId);

        const cmd: Command<LiveSensorCommand> = {
            cmd: "removelivedatasensor",
            arguments: {
                sensorId: req.data.sensorId,
                target: Application.config.id
            }
        }

        this.mqtt.publish(Application.config.mqtt.routerCommandTopic, stringifyCommand(cmd));
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
        const json = JSON.stringify(auth);

        const computed = createHash("sha256").update(json).digest("hex");

        if (computed !== hash) {
            console.log(`Unable to authorize sensor ${sensor._id}. Expected ${computed}. Received: ${hash}.`)
            return;
        }

        this.updateSubscription(auth, sensor);
    }

    private updateSubscription(request: ISensorAuthRequest, sensor: SensorModel) {
        let existing = this.sensors.get(request.sensorId);

        if (existing == undefined) {
            existing = new Subscription();
            this.sensors.set(request.sensorId, existing);
        }

        existing.add();
        console.log(`Sensor {${request.sensorId}}{${sensor.Owner}} authorized!`);

        const cmd: Command<LiveSensorCommand> = {
            cmd: "addlivedatasensor",
            arguments: {
                sensorId: request.sensorId,
                target: Application.config.id
            }
        }

        this.mqtt.publish(Application.config.mqtt.routerCommandTopic, stringifyCommand(cmd));
    }

    private async createLog() {
        let route = "";

        if (this.type === ClientType.MeasurementClient) {
            route = "/live/v1/measurements";
        }

        if (this.type === ClientType.MeasurementClient) {
            route = "/live/v1/messages";
        }

        if (this.type === ClientType.ControlMessageClient) {
            route = "/live/v1/control";
        }

        const log: AuditLog = {
            timestamp: new Date(),
            authorId: this.userId,
            route: route,
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

        if ((req.request !== "auth" && req.request !== "auth-apikey") && !this.authorized) {
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

            case "auth-apikey":
                console.debug("Received API key authorization request.")
                const authRequest = req as IWebSocketRequest<IApiKeyAuthenticationRequest>;
                this.userId = await this.keys.validateApiKey(authRequest.data.user, authRequest.data.apikey);
                this.authorized = this.userId != null;
                this.createLog();
                break;

            case "keepalive":
                this.ping(req);
                break;

            case "auth":
                await this.authorize(req.data);
                break;

            default:
                console.log(`Invalid request: ${req.request}`);
                break;
        }
    }
}
