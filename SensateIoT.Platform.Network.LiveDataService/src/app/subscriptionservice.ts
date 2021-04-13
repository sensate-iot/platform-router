import { Subscription } from "../models/subscription";

export class SubscriptionService {
    private readonly sensors: Map<string, Subscription>;

    public constructor() {
        this.sensors = new Map<string, Subscription>();
    }

    public getSubscribers() {
        return Array.from(this.sensors.keys());
    }

    public addSensor(id: string) {
        let existing = this.sensors.get(id);
        let rv = true;

        if (existing == undefined) {
            existing = new Subscription();
            this.sensors.set(id, existing);
            rv = false;
        }

        existing.add();
        console.debug(`Subscription: ${JSON.stringify(existing)}.`);

        return rv;
    }

    public removeSensor(id: string) {
        /*
         * Remove a subscription and return if any other
         * subscribers are remaining.
         */
        console.log(`Unsubscribing sensor ${id}.`);

        if (!this.sensors.has(id)) {
            console.error(`Sensor ${id} is not subscribed!`);
            return false;
        }

        const subscription = this.sensors.get(id);

        if (subscription === undefined) {
            console.error(`Unexpected empty subscription found for sensor ${id}.`)
            return false;
        }

        subscription.remove();

        console.debug(`Subscription: ${JSON.stringify(subscription)}.`);

        if (!subscription.empty()) {
            return true;
        }

        console.log(`Removing last subscription for sensor: ${id}`);
        this.sensors.delete(id);

        return false;
    }
}
