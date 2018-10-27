/*
 * Live websocket handlers.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Net.WebSockets;
using System.Threading.Tasks;

using SensateService.Infrastructure.Repositories;

namespace SensateService.WebSocketHandler
{
	public class WebSocketLiveMeasurementHandler : Middleware.WebSocketHandler
	{
		public WebSocketLiveMeasurementHandler(IWebSocketRepository sockets) : base(sockets)
		{
		}

		public override async Task Receive(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
		{
			await Task.CompletedTask;
		}
	}
}