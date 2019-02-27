/*
 * Measurement model definition.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

import { Types } from "mongoose";

export interface IDataPoint {
    Name: string;
    Value: number;
    Unit? : string;
};

export class Measurement {
    Data: [IDataPoint];
    Longitude: number;
    Latitude: number;
    CreatedAt: number;
    CreatedById: Types.ObjectId;
}
