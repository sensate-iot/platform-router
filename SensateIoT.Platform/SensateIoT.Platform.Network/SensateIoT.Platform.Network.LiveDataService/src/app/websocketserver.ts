/*
 * WebSocket handler class.
 * 
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

import * as http from "http";
import * as WebSocket from "ws";
import * as express from "express";
import * as url from "url";

import { BulkMeasurementInfo } from "../models/measurement";
import { WebSocketClient } from "../clients/websocketclient";
import { Pool } from "pg";
import { AuditLogsClient } from "../clients/auditlogsclient";
import { getClientIP } from "./util";
import { ClientType } from "../models/clienttype";
import { BulkMessageInfo } from "../models/message";
import { ApiKeyClient } from "../clients/apikeyclient";
import { MqttClient } from "../clients/mqttclient";
import { Command, stringifyCommand } from "../commands/command";
import { SyncLiveDataSensorsCommand } from "../commands/synclivedatasensorscommand";
import { Application } from "./app";
import { LiveSensorCommand } from "../commands/livesensorcommand";

export class WebSocketServer {
    private readonly server: http.Server;
    private readonly wss: WebSocket.Server;

    private readonly measurementClients: WebSocketClient[];
    private readonly messageClients: WebSocketClient[];
    private readonly auditlogs: AuditLogsClient;
    private readonly apikeys: ApiKeyClient;

    public constructor(expr: express.Express,
        private readonly pool: Pool,
        private readonly timeout: number,
        private readonly secret: string,
        private readonly mqtt: MqttClient
    ) {
        const server = http.createServer(expr);
        this.server = server;
        this.wss = new WebSocket.Server({ noServer: true });
        this.measurementClients = [];
        this.messageClients = [];
        this.auditlogs = new AuditLogsClient(pool);
        this.apikeys = new ApiKeyClient(pool);

        //setInterval(this.publicationHandler, 60000);
        setInterval(() => {
            this.publicationHandler();
        }, 60000);
    }

    private publicationHandler(): void {
        console.log("Updating message routers!");
        let sensors: string[] = [];

        this.measurementClients.forEach(m => {
            sensors = sensors.concat(m.getConnectedSensors());
        });

        this.messageClients.forEach(m => {
            sensors = sensors.concat(m.getConnectedSensors());
        });

        const cmd: Command<SyncLiveDataSensorsCommand> = {
            cmd: "synclivedatasensors",
            arguments: {
                target: Application.config.id,
                sensors: sensors
            }
        }

        this.mqtt.publish(Application.config.mqtt.routerCommandTopic, stringifyCommand(cmd));
    }

    public processMessages(messages: BulkMessageInfo) {
        this.messageClients.forEach(client => {
            if (messages.sensorId === null || messages.sensorId == undefined) {
                return;
            }

            if (!client.isServicing(messages.sensorId.toString())) {
                return;
            }

            const data = JSON.stringify(messages);
            client.process(data);
        });
    }

    public processMeasurements(measurements: BulkMeasurementInfo) {
        this.measurementClients.forEach(client => {
            if (measurements.sensorId === null || measurements.sensorId == undefined) {
                return;
            }

            if (!client.isServicing(measurements.sensorId.toString())) {
                return;
            }

            const data = JSON.stringify(measurements);
            client.process(data);
        });
    }

    public hasOpenSocketFor(sensorId: string, type: ClientType) {
        const clients = type === ClientType.MeasurementClient ?
            this.measurementClients : this.messageClients;

        for (let client of clients) {
            if (!client.isServicing(sensorId)) {
                continue;
            }

            return true;
        }

        return false;
    }

    private removeSensors(sensors: string[]) {
        if (sensors.length === 0) {
            return;
        }

        sensors.forEach(s => {
            const cmd: Command<LiveSensorCommand> = {
                cmd: "removelivedatasensor",
                arguments: {
                    sensorId: s,
                    target: Application.config.id
                }
            }

            this.mqtt.publish(Application.config.mqtt.routerCommandTopic, stringifyCommand(cmd));
        });
    }

    private destroyConnection(ws: WebSocket) {
        let done = false;
        let sensors: string[] = [];

        this.measurementClients.forEach((info, index) => {
            if (info.compareSocket(ws)) {
                console.log("Removing web socket!");
                const client = this.measurementClients.splice(index, 1);
                done = true;
                sensors = client[0].getConnectedSensors();
            }
        });


        if (done) {
            this.removeSensors(sensors);
            return;
        }

        this.messageClients.forEach((info, index) => {
            if (info.compareSocket(ws)) {
                const client = this.messageClients.splice(index, 1);
                sensors = client[0].getConnectedSensors();
            }
        });

        this.removeSensors(sensors);
    }

    private async handleSocketUpgrade(socket: WebSocket, request: any, type: ClientType) {
        let ip = getClientIP(request.headers["x-forwarded-for"]);

        if (ip === "") {
            ip = request.connection.remoteAddress.toString();
        }

        const client = new WebSocketClient(
            this.auditlogs,
            this.apikeys,
            this.secret,
            ip,
            this.timeout,
            type,
            this.mqtt,
            socket,
            this.pool);

        const hdr = request.headers["authorization"];
        console.log(`New client connection from ${ip}!`);

        if (hdr !== null && hdr !== undefined) {
            const split = hdr.split(" ");
            await client.authorize(split[1]);
        }

        switch (type) {
            case ClientType.MeasurementClient:
                this.measurementClients.push(client);
                break;

            case ClientType.MessageClient:
                this.messageClients.push(client);
                break;
        }

        this.wss.emit("connection", socket, request);
    }

    public listen(port: number) {
        this.wss.on("connection", (ws: WebSocket) => {
            ws.on("close", () => {
                this.destroyConnection(ws);
                ws.close();
            });
        });

        this.server.on("upgrade", async (request, socket, head) => {
            const pathname = url.parse(request.url).pathname;

            if (pathname === "/live/v1/measurements") {
                this.wss.handleUpgrade(request, socket, head, async (ws: WebSocket) => {
                    await this.handleSocketUpgrade(ws, request, ClientType.MeasurementClient);
                });
            } else if (pathname === "/live/v1/messages") {
                this.wss.handleUpgrade(request, socket, head, async (ws: WebSocket) => {
                    await this.handleSocketUpgrade(ws, request, ClientType.MessageClient);
                });
            }
        });

        this.server.listen(port, () => {
            console.log(`Listening on port: ${port}!`);
        });
    }
}
