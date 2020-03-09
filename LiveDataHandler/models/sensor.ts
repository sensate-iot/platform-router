/*
 * Sensor MongoDB model.
 * 
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

import { Document, Schema, model } from "mongoose";

export interface SensorModel extends Document {
    Secret: string;
    Name: string;
    Description: string;
    CreatedAt: Date;
    UpdatedAt: Date;
    Owner: string;
};

export const SensorSchema = new Schema({
    Secret: {
        type: String,
        required: true
    },
    Name: {
        type: String,
        required: true
    },
    Description: {
        type: String,
        required: true
    },
    CreatedAt: {
        type: Date,
        required: true
    },
    UpdatedAt: {
        type: Date,
        required: true
    },
    Owner: {
        type: String,
        required: true
    },
});

export const Sensor = model<SensorModel>("Sensor", SensorSchema, "Sensors");
