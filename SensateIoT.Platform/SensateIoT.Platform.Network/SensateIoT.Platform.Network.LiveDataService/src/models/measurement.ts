/*
 * Measurement model definition.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

import { Types } from "mongoose";
import { GeoJSON } from "./geojson";


export class DataPoint {
    unit: string;
    value: number;
    precision?: number;
    accuracy?: number;
}

export class Measurement {
    timestamp: Date;
    platformTime: Date;
    location: GeoJSON;
    data: Map<string, DataPoint>;
}

export class BulkMeasurementInfo {
    public measurements: Measurement[];
    public sensorId: Types.ObjectId;
}

export class MeasurementInfo {
    public measurement: Measurement;
    public sensorId: Types.ObjectId;
}
