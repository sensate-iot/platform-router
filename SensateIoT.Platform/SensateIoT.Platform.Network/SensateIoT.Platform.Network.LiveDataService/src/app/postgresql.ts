/*
 * PostgresQL connector
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { Settings } from "../models/settings";
import { Pool } from "pg";

export function connect(settings: Settings) {
    console.debug(`Connecting to PostgreSQL using: ${JSON.stringify(settings.postgresql)}`);

    return new Pool({
        max: 25,
        connectionString: settings.postgresql.connectionString
    });
}
