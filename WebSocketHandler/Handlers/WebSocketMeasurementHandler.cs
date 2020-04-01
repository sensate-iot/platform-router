/*
 * Websocket handler used to receive messages.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
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
using SensateService.Infrastructure.Events;
using SensateService.Infrastructure.Storage;
using SensateService.Models.Generic;
using SensateService.Models.Json.In;
using SensateService.Services;
using SensateService.Services.Settings;

namespace SensateService.WebSocketHandler.Handlers
{
	public class WebSocketMeasurementHandler : Middleware.WebSocketHandler
	{
		private readonly IMqttPublishService client;
		private readonly InternalMqttServiceOptions mqttopts;
		private readonly IServiceProvider provider;

		public WebSocketMeasurementHandler(IMqttPublishService client, IServiceProvider provider, IOptions<InternalMqttServiceOptions> options)
		{
			this.provider = provider;
			this.client = client;
			this.mqttopts = options.Value;
			CachedMeasurementStore.MeasurementsReceived += this.MeasurementsStored_Handler;
		}

		public override async Task Receive(AuthenticatedWebSocket socket, WebSocketReceiveResult result, byte[] buffer)
		{
			string msg;

			try {
				using var scope = this.provider.CreateScope();
				var store = scope.ServiceProvider.GetRequiredService<IMeasurementCache>();
				msg = Encoding.UTF8.GetString(buffer, 0, result.Count);

				await store.StoreAsync(msg, RequestMethod.WebSocket).AwaitBackground();
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

		private async Task MeasurementsStored_Handler(object sender, MeasurementsReceivedEventArgs e)
		{
			await this.client.PublishOnAsync(this.mqttopts.InternalBulkMeasurementTopic, e.Compressed, false).AwaitBackground();
		}
	}
}
