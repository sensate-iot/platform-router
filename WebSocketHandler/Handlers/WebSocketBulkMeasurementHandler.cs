/*
 * Websocket handler used to receive messages.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SensateService.Enums;
using SensateService.Exceptions;
using SensateService.Helpers;
using SensateService.Infrastructure.Storage;
using SensateService.Models.Generic;

namespace SensateService.WebSocketHandler.Handlers
{
	public class WebSocketBulkMeasurementHandler : Middleware.WebSocketHandler
	{
		private readonly IServiceProvider provider;

		public WebSocketBulkMeasurementHandler(IServiceProvider provider)
		{
			this.provider = provider;
		}

		public override async Task Receive(AuthenticatedWebSocket socket, WebSocketReceiveResult result, byte[] buffer)
		{
			string msg;
			IList<string> raw;


			try {
				using var scope = this.provider.CreateScope();
				var store = scope.ServiceProvider.GetRequiredService<IMeasurementCache>();
				msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
				var array = JArray.Parse(msg);

				raw = array.Select(entry => entry.ToString(Formatting.None)).ToList();

				await store.StoreRangeAsync(raw, RequestMethod.WebSocket).AwaitBackground();
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
