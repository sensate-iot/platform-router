/*
 * Message handler interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

export interface IMessageHandler {
    handle(topic: string, msg: string): Promise<void>;
    getTopic(): string;
}
