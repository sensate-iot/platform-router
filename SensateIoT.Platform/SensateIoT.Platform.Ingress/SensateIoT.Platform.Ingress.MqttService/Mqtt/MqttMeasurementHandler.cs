/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateIoT.Platform.Ingress.Common.MQTT;

namespace SensateIoT.Platform.Ingress.MqttService.Mqtt
{
	public class MqttMeasurementHandler : IMqttHandler
	{
		private readonly ILogger<MqttMeasurementHandler> logger;

		public MqttMeasurementHandler(IHttpClientFactory factory, ILogger<MqttMeasurementHandler> logger)
		{
			this.logger = logger;
		}

		public Task OnMessageAsync(string topic, string message, CancellationToken ct = default)
		{
			try {
			} catch(Exception ex) {
				this.logger.LogInformation($"Error: {ex.Message}");
				this.logger.LogInformation($"Received a buggy MQTT message: {message}");
			}

			return Task.CompletedTask;
		}
	}
}
