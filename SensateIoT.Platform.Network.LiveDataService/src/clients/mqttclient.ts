/*
 * MQTT client definition & implementation.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

import { IClientOptions, Client, connect } from "mqtt";
import { Guid } from "../app/guid";
import { IMessageHandler } from "../handlers/imessagehandler";

export class MqttClient {
    private client: Client;
    private readonly handlers: IMessageHandler[];

    public constructor(private readonly host: string,
        private readonly port: number,
        private readonly share: string) {
        this.handlers = [];
    }

    public connect(user: string, password: string) {
        console.debug(`Connecting to MQTT broker: ${this.host} on port ${this.port}`);
        const opts: IClientOptions = { };

        opts.port = this.port;
        opts.clientId = Guid.newGuid();
        opts.username = user;
        opts.password = password;

        this.client = connect(`mqtt://${this.host}`, opts);

        const rv = new Promise<void>((resolve) => {
            this.client.on("connect", () => {
                resolve();
            });
        });

        this.client.on("message", (topic, msg) => {
            if (this.share != null && this.share != "") {
                topic = `$share/${this.share}/${topic}`;
            }

            console.debug(`Received message on topic ${topic}`);

            this.handlers.forEach(handler => {
                if (handler.getTopic() !== topic) {
                    return;
                }

                handler.handle(topic, msg.toString());
            });
        });

        return rv;
    }

    public publish(topic: string, message: string) {
        this.client.publish(topic, message);
    }

    public addHandler(handler: IMessageHandler) {
        this.handlers.push(handler);
    }

    public subscribe(topic: string) {
        return new Promise((resolve, reject) => {
            this.client.subscribe(topic, (err) => {
                if (err) {
                    reject(err);
                } else {
                    resolve('');
                }
            });
        });
    }
}
