/*
 * Typescript settings model.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

const config = require('config');

export class MqttSettings {
    username: string;
    password: string;
    ssl: boolean;
    port: number;
    host: string;
    bulkMeasurementTopic: string;
    bulkMessageTopic: string;
    bulkControlMessageTopic: string;
    routerCommandTopic: string;
    topicShare: string;
}

export class MongoDB {
    connectionString: string;
    maxConnections: number;
}

export class WebServerSettings {
    port: number;
    secret: string;
    timeout: number;
}

export class PostgresQL {
    user: string;
    host: string;
    database: string;
    password: string;
    port: number;
}

export class Settings {
    mqtt: MqttSettings;
    mongoDB: MongoDB;
    postgresql: PostgresQL;
    web: WebServerSettings;
    id: string;
}

function parseMongoDB(settings: Settings) {
    settings.mongoDB.connectionString = config.get('mongodb.connectionString');
    settings.mongoDB.maxConnections = config.get('mongodb.maxConnections') as number;
}

function parseMqtt(settings: Settings) {
    settings.mqtt.username = config.get('mqtt.username');
    settings.mqtt.password = config.get('mqtt.password');
    settings.mqtt.port = config.get('mqtt.port') as number;
    settings.mqtt.ssl = config.get('mqtt.ssl') == 'true'
    settings.mqtt.host = config.get('mqtt.host');
    settings.mqtt.bulkMeasurementTopic = config.get('mqtt.bulkMeasurementTopic');
    settings.mqtt.bulkMessageTopic = config.get('mqtt.bulkMessageTopic');
    settings.mqtt.bulkControlMessageTopic = config.get('mqtt.bulkControlMessageTopic');
    settings.mqtt.routerCommandTopic = config.get('mqtt.routerCommandTopic');
    settings.mqtt.topicShare = config.get('mqtt.topicShare');
}

function parsePostgreSQL(settings: Settings) {
    settings.postgresql.database = config.get('postgresql.database');
    settings.postgresql.host = config.get('postgresql.host');
    settings.postgresql.password = config.get('postgresql.password');
    settings.postgresql.port = config.get('postgresql.port') as number;
    settings.postgresql.user = config.get('postgresql.user');
}

function parseWeb(settings: Settings) {
    settings.web.port = config.get('web.port') as number;
    settings.web.secret = config.get('web.secret');
    settings.web.timeout = config.get('web.timeout') as number;
}

export function parseSettings(): Settings {
    const settings = new Settings();

    settings.mqtt = new MqttSettings();
    settings.mongoDB = new MongoDB();
    settings.postgresql = new PostgresQL();
    settings.web = new WebServerSettings();
    settings.id = config.get('id');

    parseMqtt(settings);
    parseMongoDB(settings);
    parseWeb(settings);
    parseMqtt(settings);
    parsePostgreSQL(settings);

    return settings;
}
