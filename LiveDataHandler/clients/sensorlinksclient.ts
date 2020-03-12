/*
 * Sensor link client.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { Pool, QueryResult } from "pg";
import { SensorLink } from "../models/sensorlink";

export class SensorLinksClient {
    public constructor(private readonly pool: Pool) {
    }

    public async getSensorLinks(sensorId: string) {
        const query = `SELECT * FROM "SensorLinks" WHERE "SensorId" = '${sensorId}'`;
        let result: QueryResult;

        try {
            result = await this.pool.query(query);
        } catch (ex) {
            console.log("Unable to fetch sensor links:");
            console.log(ex);

            return [];
        }

        return result.rows as SensorLink[];
    }
}
