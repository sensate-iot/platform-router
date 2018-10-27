/*
 * Websocket connection service.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using SensateService.Middleware;

namespace SensateService.ApiCore.Middleware
{
	public class WebSocketService
	{
		private readonly RequestDelegate _next;
		private readonly WebSocketHandler _handler;

		private const int RxBufferSize = 4096;

		public WebSocketService(RequestDelegate next, WebSocketHandler handler)
		{
			this._next = next;
			this._handler = handler;
		}

		public async Task Invoke(HttpContext ctx)
		{
			WebSocket socket;

			if(!ctx.WebSockets.IsWebSocketRequest)
				return;

			socket = await ctx.WebSockets.AcceptWebSocketAsync();
			this._handler.OnConnected(socket);

			try {
				await this.Receive(socket, async(result, buffer) => {
					if(result.MessageType == WebSocketMessageType.Text) {
						await this._handler.Receive(socket, result, buffer);
					} else if(result.MessageType == WebSocketMessageType.Close) {
						await this._handler.OnDisconnected(socket);
					}
				});
			} catch(WebSocketException) {
				Debug.WriteLine($"Websocket error occurred! Socket state: {socket.State}");
				this._handler.OnForceClose(socket);
				return;
			}

			try {
				if(this._next != null)
					await this._next.Invoke(context: ctx);
			} catch(Exception) {
				return;
			}
		}

		private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
		{
			byte[] buffer = new byte[WebSocketService.RxBufferSize];

			while(socket.State == WebSocketState.Open) {
				var result = await socket.ReceiveAsync(
					new ArraySegment<byte>(buffer), CancellationToken.None
				);

				handleMessage(result, buffer);
			}
		}
	}
}
