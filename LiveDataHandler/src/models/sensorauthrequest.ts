/*
 * Sensor authorization request.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

export interface ISensorAuthRequest {
    SensorID: string;
    SensorSecret: string;
    JwtToken: string;
};
