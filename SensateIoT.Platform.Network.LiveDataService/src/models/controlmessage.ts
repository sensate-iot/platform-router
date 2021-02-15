/*
 * Control message model/DTO's.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { Types } from "mongoose";

export class ControlMessage {
    sensorId: string;
    data: string;
    secret: string;
    timestamp: Date;
}

export class BulkControlMessage {
    messages: ControlMessage[];
    sensorId: Types.ObjectId;
}
