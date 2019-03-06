/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Storage;
using SensateService.Models.Json.In;

namespace SensateService.MqttHandler.Mqtt
{
	public class MqttMeasurementHandler : Middleware.MqttHandler
	{
		private readonly IMeasurementCache store;
		private readonly ILogger<MqttMeasurementHandler> logger;

		public MqttMeasurementHandler(IMeasurementCache store, ILogger<MqttMeasurementHandler> logger)
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
			RawMeasurement raw;

			try {
				raw = JsonConvert.DeserializeObject<RawMeasurement>(message);

				if(raw.CreatedById == null)
					return;

				await this.store.StoreAsync(raw, RequestMethod.MqttTcp).AwaitBackground();
			} catch(Exception ex) {
				this.logger.LogInformation($"Error: {ex.Message}");
				this.logger.LogInformation($"Received a buggy MQTT message: {message}");
			}
		}
	}
}
