/*
 * Repository to track open websocket connections.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateService.Infrastructure.Repositories;
using System;
using System.Threading;

namespace SensateService.Infrastructure
{
	public class WebSocketRepository : IWebSocketRepository
	{
		private readonly ILogger<WebSocketRepository> _logger;
		private readonly ConcurrentDictionary<string, WebSocket> _sockets;

		public WebSocketRepository(ILogger<WebSocketRepository> logger)
		{
			this._logger = logger;
			this._sockets = new ConcurrentDictionary<string, WebSocket>();
		}

		public void Add(WebSocket socket)
		{
			this._sockets.TryAdd(this.CreateConnectionId(), socket);
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

		public void Remove(string id)
		{
			WebSocket webSocket;

			this._sockets.TryRemove(id, out webSocket);
			var result = webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
											  "Closed by SensateService",
											  cancellationToken: CancellationToken.None);
			result.RunSynchronously();
		}

		public Task RemoveAsync(string id)
		{
			WebSocket socket;

			this._sockets.TryRemove(id, out socket);
			return socket.CloseAsync(
				WebSocketCloseStatus.NormalClosure,
				"Closed by SensateService",
				cancellationToken: CancellationToken.None
			);
		}

		public void ForceRemove(WebSocket ws)
		{
			WebSocket socket;
			string key;

			key = this._sockets.FirstOrDefault(x => x.Value == ws).Key;
			this._sockets.TryRemove(key, out socket);
			socket.Dispose();
		}
	}
}
