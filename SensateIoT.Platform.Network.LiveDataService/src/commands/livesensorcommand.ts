/*
 * MQTT add/remove live sensor command interface/model.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

export interface LiveSensorCommand {
    sensorId: string;
    target: string;
}
