/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using SensateService.Helpers;
using SensateService.Infrastructure.Storage;
using SensateService.Models.Json.In;

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
			IList<RawMeasurement> raw;

			try {
				raw = JsonConvert.DeserializeObject<IList<RawMeasurement>>(message);

				await this.store.StoreRangeAsync(raw).AwaitBackground();
			} catch(Exception ex) {
				this.logger.LogInformation($"Error: {ex.Message}");
				this.logger.LogInformation($"Received a buggy MQTT message: {message}");
			}
		}
	}
}
