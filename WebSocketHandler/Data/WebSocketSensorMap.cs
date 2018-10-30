/*
 * Concurrent map, which maps one or more sockets to a sensor.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Linq;
using System.Collections.Generic;

using SensateService.Middleware;
using SensateService.Models;

namespace SensateService.WebSocketHandler.Data
{
	public class WebSocketSensorMap
	{
		private readonly List<Node> _sockets;
		private readonly object mtx;

		public WebSocketSensorMap()
		{
			this._sockets = new List<Node>();
			this.mtx = new object();
		}

		public IEnumerable<AuthenticatedWebSocket> Get(Sensor sensor)
		{
			return this.Get(sensor.InternalId.ToString());
		}

		public IEnumerable<AuthenticatedWebSocket> Get(string id)
		{
			List<Node> nodes;
			IEnumerable<AuthenticatedWebSocket> rv;

			lock(mtx) {
				var tmp = from socket in this._sockets
					where socket.SensorId == id
					select socket;
				nodes = tmp.ToList();
			}

			rv = nodes.ConvertAll(x => x.Socket);
			return rv;
		}

		private bool Contains(AuthenticatedWebSocket socket)
		{
			int count;

			lock(mtx) {
				var nodes = from s in this._sockets
					where s.Socket.Id == socket.Id
					select s;
				count = nodes.Count();

			}

			return count != 0;
		}

		public void Add(Sensor sensor, AuthenticatedWebSocket ws)
		{
			Node node;

			node = new Node(sensor.InternalId.ToString(), ws);

			lock(mtx) {
				if(this.Contains(ws))
					return;

				this._sockets.Add(node);
			}
		}

		public void Remove(AuthenticatedWebSocket ws)
		{
			lock(this.mtx) {
				this._sockets.RemoveAll(x => x.Socket.Id == ws.Id);
			}
		}

		internal class Node
		{
			public AuthenticatedWebSocket Socket { get; }
			public string SensorId { get; }

			public Node(string id, AuthenticatedWebSocket ws)
			{
				this.SensorId = id;
				this.Socket = ws;
			}
		}
	}
}
