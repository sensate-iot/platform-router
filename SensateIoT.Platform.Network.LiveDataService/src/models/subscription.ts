/*
 * Sensor subscription model.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { SensorModel } from "../models/sensor";

export class Subscription {
    private count: number;

    public Subscription(sensor: SensorModel) {
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
