/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Storage;

namespace SensateService.MqttHandler.Mqtt
{
	public class MqttBulkMeasurementHandler : Middleware.MqttHandler
	{
		private readonly IMeasurementCache store;
		private readonly ILogger<MqttMeasurementHandler> logger;

		public MqttBulkMeasurementHandler(IMeasurementCache store, ILogger<MqttMeasurementHandler> logger)
		{
			this.store = store;
			this.logger = logger;
		}

		public override void OnMessage(string topic, string msg)
		{
			Task.Run(async () => { await this.OnMessageAsync(topic, msg).AwaitBackground(); }).Wait();
		}

		public override async Task OnMessageAsync(string topic, string message)
		{
			IList<string> raw;

			try {
				var array = JArray.Parse(message);
				raw = array.Select(entry => entry.ToString(Formatting.None)).ToList();
				await this.store.StoreRangeAsync(raw, RequestMethod.WebSocket).AwaitBackground();
			} catch(Exception ex) {
				this.logger.LogInformation($"Error: {ex.Message}");
				this.logger.LogInformation($"Received a buggy MQTT message: {message}");
			}
		}
	}
}
