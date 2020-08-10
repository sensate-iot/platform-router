/*
 * Bulk measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateService.Helpers;
using SensateService.Infrastructure.Authorization;

namespace SensateService.MqttHandler.Mqtt
{
	public class MqttBulkMessageHandler : Middleware.MqttHandler
	{
		private readonly IMessageAuthorizationProxyCache m_proxy;
		private readonly ILogger<MqttBulkMessageHandler> logger;

		public MqttBulkMessageHandler(IMessageAuthorizationProxyCache store, ILogger<MqttBulkMessageHandler> logger)
		{
			this.m_proxy = store;
			this.logger = logger;
		}

		public override void OnMessage(string topic, string msg)
		{
			Task.Run(async () => { await this.OnMessageAsync(topic, msg).AwaitBackground(); }).Wait();
		}

		public override Task OnMessageAsync(string topic, string message)
		{
			try {
				this.m_proxy.AddMessages(message);
			} catch(Exception ex) {
				this.logger.LogInformation($"Error: {ex.Message}");
				this.logger.LogInformation($"Received a buggy MQTT message: {message}");
			}

			return Task.CompletedTask;
		}
	}
}
