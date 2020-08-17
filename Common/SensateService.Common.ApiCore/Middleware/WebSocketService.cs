/*
 * Websocket connection service.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using SensateService.Helpers;
using SensateService.Middleware;
using SensateService.Models.Generic;

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
			AuthenticatedWebSocket authws;

			if(!ctx.WebSockets.IsWebSocketRequest) {
				return;
			}

			var auth = await ctx.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme).AwaitBackground();
			socket = await ctx.WebSockets.AcceptWebSocketAsync().AwaitBackground();

			this._handler.OnConnected(socket);
			authws = new AuthenticatedWebSocket {
				Authentication = auth,
				Raw = socket
			};

			try {
				await Receive(authws, async (result, buffer) => {
					switch(result.MessageType) {
					case WebSocketMessageType.Text:
						await this._handler.Receive(authws, result, buffer).AwaitBackground();
						break;
					case WebSocketMessageType.Close:
						this._handler.OnDisconnected(authws);
						break;
					case WebSocketMessageType.Binary:
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(result.MessageType));
					}
				}).AwaitBackground();
			} catch(WebSocketException) {
				Debug.WriteLine($"Websocket error occurred! Socket state: {socket.State}");
				this._handler.OnForceClose(authws);
				return;
			}

			try {
				if(this._next != null)
					await this._next.Invoke(context: ctx).AwaitBackground();
			} catch(Exception) {
				// ignored
			}
		}

		private static async Task Receive(AuthenticatedWebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
		{
			byte[] buffer = new byte[RxBufferSize];

			while(socket.Raw.State == WebSocketState.Open) {
				var result = await socket.Raw.ReceiveAsync(
					new ArraySegment<byte>(buffer), CancellationToken.None
				).AwaitBackground();

				handleMessage(result, buffer);
			}
		}
	}
}
