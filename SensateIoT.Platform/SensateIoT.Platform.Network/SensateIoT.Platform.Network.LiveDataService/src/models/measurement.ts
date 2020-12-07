/*
 * Measurement model definition.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

import { Types } from "mongoose";

export interface IGeoJSON {
    type: string;
    coordinates: number[];
}

export interface IDataPoint {
    unit: string;
    value: number;
    precision?: number;
    accuracy?: number;
}

export interface Measurement {
    timestamp: Date;
    platformTime: Date;
    location: IGeoJSON;
    data: Map<string, IDataPoint>;
}

export class BulkMeasurementInfo {
    public measurements: [Measurement];
    public sensorId: Types.ObjectId;
}

export class MeasurementInfo {
    public measurement: Measurement;
    public sensorId: Types.ObjectId;
}
