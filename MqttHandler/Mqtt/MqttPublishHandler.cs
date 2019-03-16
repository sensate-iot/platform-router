/*
 * MQTT publish handler for bulk storage.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

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
		private readonly InternalMqttServiceOptions _options;

		public MqttPublishHandler(IServiceProvider provider, IOptions<InternalMqttServiceOptions> opts)
		{
			this._provider = provider;
			this._options = opts.Value;
		}

		private static Task<string> Compress(string str)
		{
			return Task.Run(() => {
				var bytes = Encoding.UTF8.GetBytes(str);
				using(var msi = new MemoryStream(bytes))
					using(var mso = new MemoryStream()) {
						using(var gs = new GZipStream(mso, CompressionMode.Compress)) {
							msi.CopyTo(gs);
						}

						return Convert.ToBase64String(mso.ToArray());
					}
			});
		}

		private async Task MeasurementsStored_Handler(object sender, MeasurementsReceivedEventArgs e)
		{
			string data;

			using(var scope = this._provider.CreateScope()) {
				var client = scope.ServiceProvider.GetRequiredService<IMqttPublishService>();

				data = JsonConvert.SerializeObject(e.Measurements);
				data = await Compress(data).AwaitBackground();
				await client.PublishOnAsync(this._options.InternalBulkMeasurementTopic, data, false).AwaitBackground();
			}
		}

		public  Task StartAsync(CancellationToken cancellationToken)
		{
			CachedMeasurementStore.MeasurementsReceived += MeasurementsStored_Handler;
			return Task.CompletedTask;
		}

		public  Task StopAsync(CancellationToken cancellationToken)
		{
			CachedMeasurementStore.MeasurementsReceived -= MeasurementsStored_Handler;
			return Task.CompletedTask;
		}
	}
}
