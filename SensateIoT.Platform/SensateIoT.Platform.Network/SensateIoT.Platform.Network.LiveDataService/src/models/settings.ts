/*
 * Typescript settings model.
 */

interface MqttSettings {
    username: string;
    password: string;
    ssl: boolean;
    port: number;
    host: string;
    bulkMeasurementTopic: string;
    bulkMessageTopic: string;
    topicShare: string;
}

interface MongoDB {
    connectionString: string;
    maxConnections: number;
}

interface WebServerSettings {
    port: number;
    secret: string;
    timeout: number;
}

interface PostgresQL {
    user: string;
    host: string;
    database: string;
    password: string;
    port: number;
}

export interface Settings {
    mqtt: MqttSettings;
    mongoDB: MongoDB;
    postgresql: PostgresQL;
    web: WebServerSettings;
}
