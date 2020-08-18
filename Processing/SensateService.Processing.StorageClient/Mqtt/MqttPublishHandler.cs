/*
 * MQTT publish handler for bulk storage.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SensateService.Helpers;
using SensateService.Infrastructure.Events;
using SensateService.Infrastructure.Storage;
using SensateService.Services;
using SensateService.Services.Settings;

namespace SensateService.Processing.StorageClient.Mqtt
{
	public class MqttPublishHandler : IHostedService
	{
		private readonly IServiceProvider _provider;
		private readonly InternalMqttServiceOptions m_opts;

		public MqttPublishHandler(IServiceProvider provider, IOptions<InternalMqttServiceOptions> opts)
		{
			this._provider = provider;
			this.m_opts = opts.Value;
		}

		private async Task MeasurementsStored_Handler(object sender, DataReceivedEventArgs e)
		{
			string data;

			using var scope = this._provider.CreateScope();
			var client = scope.ServiceProvider.GetRequiredService<IMqttPublishService>();

			data = e.Compressed;
			await client.PublishOnAsync(this.m_opts.InternalBulkMeasurementTopic, data, false).AwaitBackground();
		}

		private async Task MessagesStored_Handler(object sender, DataReceivedEventArgs e)
		{
			string data;

			using var scope = this._provider.CreateScope();
			var client = scope.ServiceProvider.GetRequiredService<IMqttPublishService>();

			data = e.Compressed;
			await client.PublishOnAsync(this.m_opts.InternalBulkMessageTopic, data, false).AwaitBackground();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			CachedMeasurementStore.MeasurementsReceived += this.MeasurementsStored_Handler;
			CachedCachedMessageStore.MessagesReceived += this.MessagesStored_Handler;
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			CachedMeasurementStore.MeasurementsReceived -= this.MeasurementsStored_Handler;
			CachedCachedMessageStore.MessagesReceived -= this.MessagesStored_Handler;
			return Task.CompletedTask;
		}
	}
}
