/*
 * MQTT add/remove live sensor command interface/model.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

export interface SyncLiveDataSensorsCommand {
    target: string;
    sensors: string[];
}
