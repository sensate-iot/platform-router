/*
 * Typescript settings model.
 */

interface MqttSettings {
    username: string;
    password: string;
    ssl: boolean;
    port: number;
    host: string;
    internalMeasurementTopic: string;
    internalBulkMeasurementTopic: string;
}

interface MongoDB {
    connectionString: string;
    maxConnections: number;
}

interface WebServerSettings {
    port: number;
    secret: string;
}

export interface Settings {
    mqtt: MqttSettings;
    mongoDB: MongoDB;
    web: WebServerSettings;
}
