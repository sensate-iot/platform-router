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

namespace SensateService.DataApi.Services
{
	public class MqttPublishHandler : IHostedService
	{
		private readonly IServiceProvider _provider;

		public MqttPublishHandler(IServiceProvider provider)
		{
			this._provider = provider;
		}

		private async Task MeasurementsStored_Handler(object sender, DataReceivedEventArgs e)
		{
			string data;

			using(var scope = this._provider.CreateScope()) {
				var opts = scope.ServiceProvider.GetRequiredService<IOptions<InternalMqttServiceOptions>>();
				var client = scope.ServiceProvider.GetRequiredService<IMqttPublishService>();

				data = e.Compressed;
				await client.PublishOnAsync(opts.Value.InternalBulkMeasurementTopic, data, false).AwaitBackground();
			}
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			CachedMeasurementStore.MeasurementsReceived += MeasurementsStored_Handler;
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			CachedMeasurementStore.MeasurementsReceived -= MeasurementsStored_Handler;
			return Task.CompletedTask;
		}
	}
}
