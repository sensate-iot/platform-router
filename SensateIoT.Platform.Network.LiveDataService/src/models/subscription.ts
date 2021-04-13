/*
 * Sensor subscription model.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

export class Subscription {
    private count: number;

    public constructor() {
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
