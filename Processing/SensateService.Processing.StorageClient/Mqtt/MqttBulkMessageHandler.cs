/*
 * MQTT handler for incoming messages.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateService.Helpers;
using SensateService.Infrastructure.Storage;

namespace SensateService.Processing.StorageClient.Mqtt
{
	public class MqttBulkMessageHandler : Middleware.MqttHandler
	{
		private readonly IMessageCache m_store;
		private readonly ILogger<MqttBulkMessageHandler> m_logger;

		public MqttBulkMessageHandler(IMessageCache cache, ILogger<MqttBulkMessageHandler> logger)
		{
			this.m_logger = logger;
			this.m_store = cache;
		}

		public override async Task OnMessageAsync(string topic, string message)
		{
			try {
				await this.m_store.StoreAsync(message).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to store message: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);
			}
		}

		public override void OnMessage(string topic, string msg)
		{
			this.OnMessageAsync(topic, msg).Wait();
		}
	}
}
