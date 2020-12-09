/*
 * MQTT handler for incoming messages.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SensateIoT.Platform.Network.Common.MQTT;

namespace SensateIoT.Platform.Network.StorageService.MQTT
{
	public class MqttBulkMessageHandler : IMqttHandler
	{
		private readonly ILogger<MqttBulkMessageHandler> m_logger;

		public MqttBulkMessageHandler(ILogger<MqttBulkMessageHandler> logger)
		{
			this.m_logger = logger;
		}

		public async Task OnMessageAsync(string topic, string message, CancellationToken ct)
		{
			try {
				//await this.m_store.StoreAsync(message).AwaitBackground();
				Console.WriteLine(message);
				await Task.CompletedTask;
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to store message: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);
			}
		}
	}
}
