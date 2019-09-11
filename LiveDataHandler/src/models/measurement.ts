/*
 * Measurement model definition.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

import { Types } from "mongoose";

export interface IDataPoint {
    Name: string;
    Value: number;
    Unit? : string;
};

export class Measurement {
    public Data: [IDataPoint];
    public Longitude: number;
    public Latitude: number;
    public CreatedAt: number;
}

export class BulkMeasurementInfo {
    public Measurements: [Measurement];
    public CreatedBy: Types.ObjectId;
}

export class MeasurementInfo {
    public Measurement: Measurement;
    public CreatedBy: Types.ObjectId;
}
