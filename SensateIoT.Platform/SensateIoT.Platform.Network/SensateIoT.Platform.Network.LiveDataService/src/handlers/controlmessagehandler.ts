/*
 * Control message handler.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { IMessageHandler } from "./imessagehandler";
import { WebSocketServer } from "../app/websocketserver";
import * as gzip from "zlib";
import { sensateiot } from "../../generated/proto";
import { ClientType } from "../models/clienttype";
import { BulkControlMessage, ControlMessage } from "../models/controlmessage";
import { ObjectId } from "mongodb";

export class ControlMessageHandler implements IMessageHandler {
    public constructor(private readonly wss: WebSocketServer, private readonly topic: string) {
    }

    public async handle(topic: string, data: string) {
        const msg = await this.decode(data);
        const messages = sensateiot.ControlMessageData.decode(msg);
        const blocks = this.groupMessages(messages);

        console.debug("Handling control message live data.");

        blocks.forEach(block => {
            if (!this.wss.hasOpenSocketFor(block.sensorId.toString(), ClientType.ControlMessageClient)) {
                return;
            }

            console.debug("Writing control messages to client.");
            this.wss.processControlMessages(block);
        });
    }

    private decode(data: string) {
        const buf = Buffer.from(data, "base64");

        return new Promise<Buffer>((resolve, reject) => {
            gzip.unzip(buf, (err, buffer) => {
                if (err) {
                    reject(err);
                }

                resolve(buffer);
            });
        });
    }

    private groupMessages(messages: sensateiot.ControlMessageData): BulkControlMessage[] {
        const result: BulkControlMessage[] = [];

        const sorted = messages.Messages.sort((a, b) => {
            if (a.SensorID < b.SensorID) {
                return -1;
            }

            if (a.SensorID > b.SensorID) {
                return 1;
            }

            return 0;
        });

        let dict = {};

        sorted.forEach((el) => {
            if (el.SensorID in dict) {
                dict[el.SensorID].push(el);
            } else {
                dict[el.SensorID] = [];
                dict[el.SensorID].push(el);
            }
        });

        for (let id in dict) {
            const block = new BulkControlMessage();

            block.messages = new Array<ControlMessage>();
            block.sensorId = ObjectId.createFromHexString(id);

            dict[id].forEach((elem: sensateiot.ControlMessage) => {
                const m = new ControlMessage();

                m.data = elem.Data;
                m.secret = elem.Secret;
                m.sensorId = id;
                m.timestamp = new Date((elem.Timestamp.seconds as number * 1000) + (elem.Timestamp.nanos / 1e6));

                block.messages.push(m)
            });

            result.push(block);
        }

        return result;
    }



    public getTopic(): string {
        return this.topic;
    }
}
