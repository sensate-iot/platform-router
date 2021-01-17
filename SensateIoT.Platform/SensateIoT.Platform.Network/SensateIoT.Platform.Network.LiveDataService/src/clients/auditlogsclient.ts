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
        let query;

        if (log.authorId === "" || log.authorId == null) {
            query = `INSERT INTO "AuditLogs" ("Route", "Method", "Address", "AuthorId", "Timestamp") VALUES ` +
                `('${log.route}', ${log.method}, '${log.ipAddress}', NULL, NOW())`;
        } else {
            query = `INSERT INTO "AuditLogs" ("Route", "Method", "Address", "AuthorId", "Timestamp") VALUES ` +
                `('${log.route}', ${log.method}, '${log.ipAddress}', '${log.authorId}', NOW())`;
        }

        try {
            await this.pool.query(query);
        } catch (ex) {
            console.log(`Unable to create audit log entry (User: ${log.authorId}`);
            console.debug(ex);
        }
    }
}
