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
        // INSERT INTO "AuditLogs" ("Route", "Method", "Address", "AuthorId", "Timestamp") VALUES ('/bla', 2, '127.0.0.1', NULL, NOW())
        const author = log.authorId == null ? "NULL" : log.authorId;
        const query = `INSERT INTO "AuditLogs" ("Route", "Method", "Address", "AuthorId", "Timestamp") VALUES ` +
            `('${log.route}', ${log.method}, '${log.ipAddress}', '${author}', NOW())`;
        try {
            await this.pool.query(query);
        } catch (ex) {
            console.log("Unable to create audit log entry:");
            console.log(ex);
        }
    }
}
