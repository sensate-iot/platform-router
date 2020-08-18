/*
 * Websocket handler used to receive messages.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using SensateService.Common.Data.Dto.Generic;
using SensateService.Exceptions;
using SensateService.Infrastructure.Authorization;

namespace SensateService.Ingress.WebSocketHandler.Handlers
{
	public class WebSocketMeasurementHandler : Middleware.WebSocketHandler
	{
		private readonly IServiceProvider provider;

		public WebSocketMeasurementHandler(IServiceProvider provider)
		{
			this.provider = provider;
		}

		public override async Task Receive(AuthenticatedWebSocket socket, WebSocketReceiveResult result, byte[] buffer)
		{
			string msg;

			try {
				using var scope = this.provider.CreateScope();
				var store = scope.ServiceProvider.GetRequiredService<IMeasurementAuthorizationProxyCache>();
				msg = Encoding.UTF8.GetString(buffer, 0, result.Count);

				store.AddMessage(msg);
			} catch(InvalidRequestException ex) {
				Debug.WriteLine($"Unable to store measurement: {ex.Message}");
				dynamic jobj = new JObject();
				jobj.status = ex.ErrorCode;
				await this.SendMessage(socket.Raw, jobj.ToString());
			} catch(Exception ex) {
				Debug.WriteLine($"Unable to store measurement: {ex.Message}");
				dynamic jobj = new JObject();
				jobj.status = 500;
				await this.SendMessage(socket.Raw, jobj.ToString());
			}
		}
	}
}
