/*
 * Repository to track open websocket connections.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Threading;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

using SensateService.Infrastructure.Repositories;

namespace SensateService.Infrastructure
{
	public class WebSocketRepository : IWebSocketRepository
	{
		private readonly ConcurrentDictionary<string, WebSocket> _sockets;

		public WebSocketRepository()
		{
			this._sockets = new ConcurrentDictionary<string, WebSocket>();
		}

		public void Add(WebSocket socket)
		{
			this._sockets.TryAdd(this.CreateConnectionId(), socket);
		}

		public void Add(string id, WebSocket socket)
		{
			if(!this._sockets.TryAdd(id, socket)) {
				throw new ArgumentException("ID already exists!");
			}
		}

		private string CreateConnectionId()
		{
			return Guid.NewGuid().ToString();
		}

		public IEnumerable<WebSocket> GetAll()
		{
			return this._sockets.Values.AsEnumerable();
		}

		public WebSocket GetById(string id)
		{
			return this._sockets[id];
		}

		public string GetId(WebSocket socket)
		{
			return this._sockets.FirstOrDefault(kv => kv.Value == socket).Key;
		}

		public Task RemoveAsync(string id)
		{
			this._sockets.TryRemove(id, out var socket);
			return socket.CloseAsync(
				WebSocketCloseStatus.NormalClosure,
				"Closed by SensateService",
				cancellationToken: CancellationToken.None
			);
		}

		public void ForceRemove(WebSocket ws)
		{
			string key;

			key = this._sockets.FirstOrDefault(x => x.Value == ws).Key;
			this._sockets.TryRemove(key, out var socket);
			socket.Dispose();
		}
	}
}
