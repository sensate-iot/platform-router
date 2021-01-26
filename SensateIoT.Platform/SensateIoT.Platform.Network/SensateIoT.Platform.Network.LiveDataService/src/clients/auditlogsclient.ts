/*
 * Audit log data model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

import { Pool } from "pg";
import { AuditLog } from "../models/auditlog";

export class AuditLogsClient {
    public constructor(private readonly pool: Pool) {
    }

    public async createEntry(log: AuditLog) {
        try {
            this.pool.connect().then(client => {
                client.query('SELECT * FROM livedataservice_createauditlog($1, $2, $3, $4)', [log.route, log.method, log.ipAddress, log.authorId]).then(res => {
                    client.release();
                });
            });
        } catch (ex) {
            console.log(`Unable to create audit log entry (User: ${log.authorId}`);
            console.debug(ex);
        }
    }
}
