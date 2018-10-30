/*
 * Keep track of open web sockets.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SensateService.Infrastructure.Repositories
{
	public interface IWebSocketRepository
	{
		WebSocket GetById(string id);
		string GetId(WebSocket socket);
		void Add(WebSocket socket);
		void Add(string id, WebSocket socket);
		Task RemoveAsync(string id);
		void ForceRemove(WebSocket ws);
		IEnumerable<WebSocket> GetAll();
	}
}
