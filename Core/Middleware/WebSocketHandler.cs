/*
 * Websocket connection handler.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SensateService.Infrastructure.Repositories;

namespace SensateService.Middleware
{
	public abstract class WebSocketHandler
	{
		protected readonly IWebSocketRepository _sockets;

		public WebSocketHandler(IWebSocketRepository socketRepository)
		{
			this._sockets = socketRepository;
		}

		public virtual void OnConnected(WebSocket socket)
		{
			this._sockets.Add(socket);
		}

		public virtual async Task OnDisconnected(WebSocket socket)
		{
			var id = this._sockets.GetId(socket);
			await this._sockets.RemoveAsync(id);
		}

		public virtual void OnForceClose(WebSocket socket)
		{
			this._sockets.ForceRemove(socket);
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

		public async Task SendMessage(string id, string message)
		{
			WebSocket webSocket;

			webSocket = this._sockets.GetById(id);
			await this.SendMessage(webSocket, message);
		}

		public async Task BroadcastMessage(string message)
		{
			IEnumerable<WebSocket> sockets;

			sockets = this._sockets.GetAll();
			foreach(var socket in sockets) {
				if(socket.State == WebSocketState.Open)
					await this.SendMessage(socket, message);
			}
		}

		public abstract Task Receive(WebSocket socket, WebSocketReceiveResult result, byte[] buffer);
	}
}
