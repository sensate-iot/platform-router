/*
 * WebSocket handler class.
 * 
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

import * as http from "http";
import * as WebSocket from "ws";
import * as express from "express";
import * as jwt from "jsonwebtoken";
import * as url from "url";
import { Types } from "mongoose";
import { ISensorAuthRequest } from "./models/sensorauthrequest";
import { Sensor } from "./models/sensor";
import { Measurement } from "./models/measurement";

export function createServer(expr: express.Express) {
    const server = http.createServer(expr);
    return server;
}

interface SocketInfo {
    uid?: string;
    jwt?: string;
    sensorid?: string;
    ws: any;
    auth: boolean;
};

export class WebSocketServer {
    private readonly server: http.Server;
    private readonly wss: WebSocket.Server;

    private retainedConnectionInfo: SocketInfo[] = [];

    constructor(expr: express.Express, private secret: string) {
        const server = http.createServer(expr);
        this.server = server;
        this.wss = new WebSocket.Server({ noServer: true });
    }

    public onMeasurementReceived(id: Types.ObjectId, measurement: Measurement) {
        this.retainedConnectionInfo.forEach((socket) => {
            if (id.toString() === socket.sensorid) {
                socket.ws.send(JSON.stringify(measurement));
            }
        });
    }

    private onMessage(ws: any, data : WebSocket.Data) {
        const info = this.findConnectionInfoBySocket(ws);

        if (info.auth)
            return;

        const request: ISensorAuthRequest = JSON.parse(data.toString());
        jwt.verify(request.JwtToken, this.secret, (err, decoded: any) => {
            if (err) {
                this.destroyConnection(ws);
                ws.close();
                return;
            }

            info.uid = decoded.nameid;
            info.jwt = request.JwtToken;
            info.sensorid = request.SensorID;
        });

        this.authorizeSensor(info, request.SensorSecret).then(() => {
            console.log("Socket authorized!");
            info.auth = true;
        }).catch((err) => {
            console.log(`Failed to authorize socket: ${err.toString()}`);
        });
    }

    private async authorizeSensor(obj: SocketInfo, secret: string) {
        if(obj.auth)
            return true;

        return new Promise(async(resolve, reject) => {
            const sensor = await Sensor.findById(obj.sensorid);

            if (sensor.Secret !== secret) {
                reject();
            }

            resolve();
        });
    }

    private destroyConnection(ws: any) {
        this.retainedConnectionInfo.forEach((info, index) => {
            if (info.ws === ws) {
                console.log("Removing web socket!");
                this.retainedConnectionInfo.splice(index, 1);
            }
        });
    }

    private findConnectionInfoBySocket(socket: any) {
        let value : SocketInfo = null;
        this.retainedConnectionInfo.forEach((info) => {
            if (info.ws === socket) {
                value = info;
            }
        });

        return value;
    }

    public listen(port: number) {
        this.wss.on("connection", (ws) => {
            ws.on("message", data => {
                this.onMessage(ws, data);
            });

            ws.on("close", () => {
                this.destroyConnection(ws);
                ws.close();
            });
        });

        this.server.on("upgrade", (request, socket, head) => {
            const pathname = url.parse(request.url).pathname;

            if (pathname === "/measurements/live") {
                this.wss.handleUpgrade(request, socket, head, ws => {
                    const info: SocketInfo = {
                        ws: ws,
                        auth: false
                };

                    this.retainedConnectionInfo.push(info);
                    this.wss.emit("connection", ws, request);
                });
            }
        });

        this.server.listen(port, () => {
            console.log(`Listening on port: ${port}!`);
        });
    }
}
