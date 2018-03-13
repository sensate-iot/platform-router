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

namespace SensateService.Infrastructure
{
	public class WebSocketRepository : IWebSocketRepository
	{
		private readonly ILogger<WebSocketRepository> _logger;
		private readonly IDictionary<string, WebSocket> _sockets;

		public WebSocketRepository(ILogger<WebSocketRepository> logger)
		{
			this._logger = logger;
			this._sockets = new ConcurrentDictionary<string, WebSocket>();
		}

		public void Add(WebSocket socket)
		{
			this._sockets.Add(this.CreateConnectionId(), socket);
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
			this._sockets.Remove(id);
		}

		public Task RemoveAsync(string id)
		{
			return Task.Run(() => {
				this.Remove(id);
			});
		}
	}
}
