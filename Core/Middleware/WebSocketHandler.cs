/*
 * Websocket connection handler.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SensateService.Models.Generic;

namespace SensateService.Middleware
{
	public abstract class WebSocketHandler
	{
		protected WebSocketHandler()
		{
		}

		public virtual void OnConnected(WebSocket socket)
		{
		}

		public virtual void OnDisconnected(AuthenticatedWebSocket socket)
		{
		}

		public virtual void OnForceClose(AuthenticatedWebSocket socket)
		{
		}

		public async Task SendMessage(WebSocket socket, string message)
		{
			if(socket.State != WebSocketState.Open)
				return;

			await socket.SendAsync(
				new ArraySegment<byte>(Encoding.ASCII.GetBytes(message),
					0, message.Length),
				WebSocketMessageType.Text,
				true,
				CancellationToken.None
			);
		}

		public abstract Task Receive(AuthenticatedWebSocket socket, WebSocketReceiveResult result, byte[] buffer);
	}
}
