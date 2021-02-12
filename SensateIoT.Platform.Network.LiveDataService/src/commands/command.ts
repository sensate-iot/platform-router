/*
 * MQTT command interface/model.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

export interface Command<T> {
    cmd: string;
    arguments: T;
}

interface InternalCommand {
    cmd: string;
    arguments: string;
}

export function stringifyCommand<T>(cmd: Command<T>): string {
    const args = JSON.stringify(cmd.arguments);
    const internal: InternalCommand = {
        arguments: args,
        cmd: cmd.cmd
    };

    return JSON.stringify(internal);
}
