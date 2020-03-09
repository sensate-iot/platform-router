/*
 * Sensor authorization request.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

export interface ISensorAuthRequest {
    sensorId: string;
    sensorSecret: string;
    timestamp: string;
}
