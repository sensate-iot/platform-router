/*
 * Sensor link client.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { Pool } from "pg";
import { SensorLink } from "../models/sensorlink";

export class SensorLinksClient {
    public constructor(private readonly pool: Pool) {
    }

    public async getSensorLinks(sensorId: string) {
        const query = `SELECT * FROM "SensorLinks" WHERE "SensorId" = '${sensorId}'`;
        const result = await this.pool.query(query);

        return result.rows as SensorLink[];
    }
}
