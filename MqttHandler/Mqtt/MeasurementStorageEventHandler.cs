/*
 * Handle the reception of a new measurement.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Events;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Services;

namespace SensateService.MqttHandler.Mqtt
{
	public class MeasurementStorageEventHandler
	{
		private readonly ILogger<MeasurementStorageEventHandler> _logger;
		private readonly IMqttPublishService _client;
		private readonly MqttServiceOptions _opts;
		private readonly IServiceProvider _provider;

		public bool Cancelled { get; set; }

		public MeasurementStorageEventHandler(
			IOptions<MqttServiceOptions> options,
			IServiceProvider provider, IMqttPublishService client
		)
		{ 
			this._logger = provider.GetRequiredService<ILogger<MeasurementStorageEventHandler>>();
			this._client = client;
			this._opts = options.Value;
			this._provider = provider;

			this.Cancelled = false;
		}


#if DEBUG
		public Task MeasurementReceived_DebugHandler(object sender, MeasurementReceivedEventArgs e)
		{
			if(!(sender is Sensor sensor))
				return Task.CompletedTask;

			this._logger.LogInformation($"Received measurement from {{{sensor.Name}}}:{{{sensor.InternalId}}}");
			return Task.CompletedTask;
		}
#endif

		public async Task InternalMqttMeasurementPublish_Handler(object sender, MeasurementReceivedEventArgs e)
		{
			string msg;

			msg = e.Measurement.ToJson();
			await this._client.PublishOnAsync(this._opts.InternalMeasurementTopic, msg, false);
		}

		public async Task MeasurementReceived_Handler(object sender, MeasurementReceivedEventArgs e)
		{
			SensateUser user;
			AuditLog log;

			if(this.Cancelled)
				throw new ObjectDisposedException("MeasurementHandler");

			if(!(sender is Sensor sensor))
				return;

			try {
				using(var scope = this._provider.CreateScope()) {
					var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
					var auditlogs = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();

					user = await users.GetAsync(sensor.Owner);
					log = new AuditLog {
						Address = IPAddress.Any,
						Method = RequestMethod.MqttTcp,
						Route = "MQTT",
						Timestamp = DateTime.Now,
						Author = user
					};

					await auditlogs.CreateAsync(log, e.CancellationToken).AwaitSafely();
				}
			} catch (Exception ex) {
				this._logger.LogInformation("Error: " + ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
		}
	}
}
