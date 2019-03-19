/*
 * MQTT client definition & implementation.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

import { IClientOptions, Client, connect } from "mqtt";
import { Guid } from "../guid";
import * as gzip from "zlib";

declare type MessageHandler = (topic: string, message: string) => void;

export class MqttClient {
    private client: Client;
    private handler: MessageHandler;
    constructor(private readonly host: string, private readonly port: number) { }

    private decode(data: string) {
        const buf = Buffer.from(data, "base64");

        return new Promise((resolve, reject) => {
            gzip.unzip(buf, (err, buffer) => {
                if (err)
                    reject(err);

                const content = buffer.toString('utf-8');
                resolve(content);
            });
        });
    }

    public connect(user: string, password: string) {
        console.debug(`Connecting to MQTT broker: ${this.host} on port ${this.port}`);
        const opts: IClientOptions = { };

        opts.port = this.port;
        opts.clientId = Guid.newGuid();
        opts.username = user;
        opts.password = password;

        this.client = connect(`mqtt://${this.host}`, opts);
        const rv = new Promise((resolve) => {
            this.client.on("connect", () => {
                resolve();
            });
        });

        this.client.on("message", (topic, msg) => {
            this.decode(msg.toString()).then((data: string) => {
                this.handler(topic, data);
            }).catch(err => {
                console.warn(`Unable to decode MQTT message: ${err.toString()}`);
            });
        });

        return rv;
    }

    public publish(topic: string, message: string) {
        this.client.publish(topic, message);
    }

    public setHandleMessage(msg: MessageHandler) {
        this.handler = msg;
    }

    public subscribe(topic: string) {
        return new Promise((resolve, reject) => {
            this.client.subscribe(topic, (err) => {
                if (err)
                    reject(err);
                else
                    resolve();
            });
        });
    }
}
