/*
 * WebSocket request type.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

export interface IWebSocketRequest<T> {
    request: string;
    data: T;
}