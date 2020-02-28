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

import { ISocketAuthRequest } from "../models/sensorauthrequest";
import { BulkMeasurementInfo } from "../models/measurement";
import { WebSocketClient } from "../clients/websocketclient";

export function createServer(expr: express.Express) {
    const server = http.createServer(expr);
    return server;
}

export class WebSocketServer {
    private readonly server: http.Server;
    private readonly wss: WebSocket.Server;

    private readonly clients: WebSocketClient[];

    constructor(expr: express.Express, private secret: string) {
        const server = http.createServer(expr);
        this.server = server;
        this.wss = new WebSocket.Server({ noServer: true });
        this.clients = [];
    }

    public process(measurements: BulkMeasurementInfo) {
        this.clients.forEach(client => {
            if (!client.isServicing(measurements.CreatedBy.toString())) {
                return;
            }

            client.process(measurements);
        });
    }

    public hasOpenSocketFor(sensorId: string) {
        for (let client of this.clients) {
            if (!client.isServicing(sensorId)) {
                continue;
            }

            return true;
        }

        return false;
    }

    private onMessage(ws: any, data : WebSocket.Data) {
        const socket = this.findClient(ws);

        if (socket !== null) {
            return;
        }

        const request: ISocketAuthRequest = JSON.parse(data.toString());
        jwt.verify(request.jwtToken, this.secret, (err) => {
            if (err) {
                this.destroyConnection(ws);
                ws.close();
                console.log(`JWT authentication failed: ${err.message}`);
                return;
            }

            this.clients.push(new WebSocketClient(ws));
        });
    }

    private destroyConnection(ws: WebSocket) {
        this.clients.forEach((info, index) => {
            if (info.compareSocket(ws)) {
                console.log("Removing web socket!");
                this.clients.splice(index, 1);
            }
        });
    }

    private findClient(socket: WebSocket) {
        for (let client of this.clients) {
            if (!client.compareSocket(socket)) {
                continue;
            }

            return client;
        }

        return null;
    }

    public listen(port: number) {
        this.wss.on("connection", (ws: WebSocket) => {
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
                    this.wss.emit("connection", ws, request);
                });
            }
        });

        this.server.listen(port, () => {
            console.log(`Listening on port: ${port}!`);
        });
    }
}
