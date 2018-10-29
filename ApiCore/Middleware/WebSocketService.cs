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

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

using SensateService.Middleware;

namespace SensateService.ApiCore.Middleware
{
	public class WebSocketService
	{
		private readonly RequestDelegate _next;
		private readonly WebSocketHandler _handler;
		private readonly IServiceProvider _provider;

		private const int RxBufferSize = 4096;

		public WebSocketService(RequestDelegate next, WebSocketHandler handler, IServiceProvider sp)
		{
			this._next = next;
			this._handler = handler;
			this._provider = sp;
		}

		public async Task Invoke(HttpContext ctx)
		{
			WebSocket socket;
			AuthenticatedWebSocket authws;

			if(!ctx.WebSockets.IsWebSocketRequest)
				return;

			var auth = await ctx.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
			socket = await ctx.WebSockets.AcceptWebSocketAsync();

			if(auth == null || auth.None || !auth.Succeeded) {
				await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Unable to authenticate client!", CancellationToken.None);
				return;
			}

			this._handler.OnConnected(socket);
			authws = new AuthenticatedWebSocket {
				Authentication = auth,
				Raw = socket
			};

			try {
				await this.Receive(authws, async(result, buffer) => {
					switch(result.MessageType) {
						case WebSocketMessageType.Text:
							await this._handler.Receive(authws, result, buffer);
							break;
						case WebSocketMessageType.Close:
							await this._handler.OnDisconnected(socket);
							break;
						case WebSocketMessageType.Binary:
							break;
						default:
							throw new ArgumentOutOfRangeException();
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
				// ignored
			}
		}

		private async Task Receive(AuthenticatedWebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
		{
			byte[] buffer = new byte[WebSocketService.RxBufferSize];

			while(socket.Raw.State == WebSocketState.Open) {
				var result = await socket.Raw.ReceiveAsync(
					new ArraySegment<byte>(buffer), CancellationToken.None
				);

				handleMessage(result, buffer);
			}
		}
	}
}
