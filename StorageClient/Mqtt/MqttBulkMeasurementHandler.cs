/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateService.Helpers;
using SensateService.Infrastructure.Storage;

namespace SensateService.MqttHandler.Mqtt
{
	public class MqttBulkMeasurementHandler : Middleware.MqttHandler
	{
		private readonly IMeasurementCache store;
		private readonly ILogger<MqttBulkMeasurementHandler> logger;

		public MqttBulkMeasurementHandler(IMeasurementCache store, ILogger<MqttBulkMeasurementHandler> logger)
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
			try {
				await this.store.StoreAsync(message).AwaitBackground();
			} catch(Exception ex) {
				this.logger.LogInformation($"Error: {ex.Message}");
				this.logger.LogInformation($"Received a buggy MQTT message: {message}");
			}
		}
	}
}
