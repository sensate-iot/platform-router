/*
 * MQTT client definition & implementation.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

import { IClientOptions, Client, connect } from "mqtt";
import { Guid } from "../guid";

declare type MessageHandler = (topic: string, message: string) => void;

export class MqttClient {
    private client: Client;
    private handler: MessageHandler;
    constructor(private host: string, private port: number) { }

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
            this.handler(topic, msg.toString());
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
