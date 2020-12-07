/*
 * PostgresQL connector
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { Settings } from "../models/settings";
import { Pool } from "pg";

export function connect(settings: Settings) {
    return new Pool({
        user: settings.postgresql.user,
        host: settings.postgresql.host,
        database: settings.postgresql.database,
        password: settings.postgresql.password,
        port: settings.postgresql.port
    });
}
