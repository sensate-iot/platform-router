/*
 * Audit log data model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

export interface AuditLog {
    route: string;
    method: number;
    ipAddress: string;
    authorId: string;
    timestamp: Date;
}

export enum RequestMethod {
    HttpGet,
    HttpPost,
    HttpPatch,
    HttpPut,
    HttpDelete,

    WebSocket,

    MqttTcp,
    MqttWebSocket,
    Any
}
