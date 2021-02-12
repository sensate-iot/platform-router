/*
 * Single message handler.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { IMessageHandler } from "./imessagehandler";
import { WebSocketServer } from "../app/websocketserver";
import * as gzip from "zlib";
import { sensateiot } from "../../generated/proto";
import { BulkMessageInfo, Message } from "../models/message";
import { ClientType } from "../models/clienttype";
import { ObjectId } from "mongodb";

export class MessageHandler implements IMessageHandler {
    public constructor(private readonly wss: WebSocketServer, private readonly topic: string) {
    }

    public async handle(topic: string, data: string) {
        const msg = await this.decode(data);
        const messages = sensateiot.TextMessageData.decode(msg);
        const blocks = this.groupMessages(messages);

        blocks.forEach(block => {
            if (!this.wss.hasOpenSocketFor(block.sensorId.toString(), ClientType.MessageClient)) {
                return;
            }

            this.wss.processMessages(block);
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

    private groupMessages(messages: sensateiot.TextMessageData): BulkMessageInfo[] {
        const result: BulkMessageInfo[] = [];

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
            const block = new BulkMessageInfo();

            block.messages = new Array<Message>();
            block.sensorId = ObjectId.createFromHexString(id);

            dict[id].forEach((elem: sensateiot.TextMessage) => {
                const m = new Message();

                m.data = elem.Data;

                m.location = {
                    type: 'point',
                    coordinates: [elem.Longitude, elem.Latitude]
                };

                m.platformTime = new Date((elem.PlatformTime.seconds as number * 1000) + (elem.PlatformTime.nanos / 1e6));
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
