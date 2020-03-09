/*
 * Websocket handler used to receive messages.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SensateService.Enums;
using SensateService.Exceptions;
using SensateService.Helpers;
using SensateService.Infrastructure.Storage;
using SensateService.Models.Generic;
using SensateService.Models.Json.In;
using SensateService.Services;
using SensateService.Services.Settings;

namespace SensateService.WebSocketHandler.Application
{
	public class RealTimeWebSocketMeasurementHandler : Middleware.WebSocketHandler
	{
		private readonly IServiceProvider provider;

		public RealTimeWebSocketMeasurementHandler(IMqttPublishService client, IServiceProvider provider, IOptions<MqttServiceOptions> options)
		{
			this.provider = provider;
		}

		public override async Task Receive(AuthenticatedWebSocket socket, WebSocketReceiveResult result, byte[] buffer)
		{
			string msg;
			JObject raw;

			msg = Encoding.UTF8.GetString(buffer, 0, result.Count);

			try {
				using var scope = this.provider.CreateScope();
				var store = scope.ServiceProvider.GetRequiredService<IMeasurementStore>();
				var reader = new JsonTextReader(new StringReader(msg)) { FloatParseHandling = FloatParseHandling.Decimal };

				raw = JObject.Load(reader);

				await store.StoreAsync(raw, RequestMethod.WebSocket).AwaitBackground();
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
