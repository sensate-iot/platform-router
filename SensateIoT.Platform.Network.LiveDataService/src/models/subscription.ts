/*
 * Sensor subscription model.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { Sensor, SensorModel } from "../models/sensor";

export class Subscription {
    public sensor: SensorModel;
    public count: number;

    public Subscription() {
        this.count = 0;
    }

    public add() {
        this.count += 1;
    }

    public remove() {
        this.count -= 1;
    }

    public empty() {
        return this.count <= 0;
    }
}
