/*
 * Measurement model definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { Types } from "mongoose";
import { GeoJSON } from "./geojson";

export class Message {
    timestamp: Date;
    platformTime: Date;
    location: GeoJSON;
    data: string;
}

export class BulkMessageInfo {
    messages: Message[];
    sensorId: Types.ObjectId;
}
