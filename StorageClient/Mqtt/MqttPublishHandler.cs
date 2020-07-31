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

namespace SensateService.MqttHandler.Mqtt
{
	public class MqttPublishHandler : IHostedService
	{
		private readonly IServiceProvider _provider;

		public MqttPublishHandler(IServiceProvider provider)
		{
			this._provider = provider;
		}

		private async Task MeasurementsStored_Handler(object sender, MeasurementsReceivedEventArgs e)
		{
			string data;

			using var scope = this._provider.CreateScope();
			var client = scope.ServiceProvider.GetRequiredService<IMqttPublishService>();

			data = e.Compressed;
			//await client.PublishOnAsync(this._options.InternalBulkMeasurementTopic, data, false).AwaitBackground();
			await client.PublishOnAsync("sensate/measurements", data, false).AwaitBackground();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			CachedMeasurementStore.MeasurementsReceived += this.MeasurementsStored_Handler;
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			CachedMeasurementStore.MeasurementsReceived -= this.MeasurementsStored_Handler;
			return Task.CompletedTask;
		}
	}
}
