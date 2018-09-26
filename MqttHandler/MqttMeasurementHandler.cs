/*
 * MQTT message handler.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;

using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Events;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.In;

namespace SensateService.MqttHandler
{
	public class MqttMeasurementHandler : Middleware.MqttHandler, IDisposable
	{
		private readonly ISensorRepository sensors;
		private readonly IMeasurementRepository measurements;
		private readonly IAuditLogRepository auditlogs;
		private readonly IUserRepository users;
		private readonly ISensorStatisticsRepository stats;

		private bool disposed;

		public MqttMeasurementHandler(ISensorRepository sensors,
									  IMeasurementRepository measurements,
									  IAuditLogRepository auditlogs,
									  ISensorStatisticsRepository stats,
									  IUserRepository users)
		{
			this.sensors = sensors;
			this.measurements = measurements;
			this.auditlogs = auditlogs;
			this.users = users;
			this.stats = stats;

			this.measurements.MeasurementReceived += this.MeasurementReceived_Handler;
#if DEBUG
			this.measurements.MeasurementReceived += this.MeasurementReceived_DebugHandler;
#endif
			this.disposed = false;
		}

#if DEBUG
		public Task MeasurementReceived_DebugHandler(object sender, MeasurementReceivedEventArgs e)
		{
			if(!(sender is Sensor sensor))
				return Task.CompletedTask;

			Console.WriteLine($"Received measurement from {{{sensor.Name}}}:{{{sensor.InternalId}}}");
			return Task.CompletedTask;
		}
#endif

		private async Task MeasurementReceived_Handler(object sender, MeasurementReceivedEventArgs e)
		{
			SensateUser user;
			AuditLog log;

			if(this.disposed)
				throw new ObjectDisposedException("MeasurementHandler");

			if(!(sender is Sensor sensor))
				return;

			try {
				user = this.users.Get(sensor.Owner);
				log = new AuditLog {
					Address = IPAddress.Any,
					Method = RequestMethod.MqttTcp,
					Route = "NA",
					Timestamp = DateTime.Now,
					Author = user
				};

				await this.auditlogs.CreateAsync(log, e.CancellationToken);
			} catch (Exception ex) {
				Console.WriteLine(ex.Message);
				Console.WriteLine("");
				Console.WriteLine(ex.StackTrace);
			}
		}

		public override void OnMessage(string topic, string msg)
		{
			Task t;

			t = this.OnMessageAsync(topic, msg);
			t.Wait();
		}

		public override async Task OnMessageAsync(string topic, string message)
		{
			Sensor sensor;
			RawMeasurement raw;

			if(this.disposed)
				throw new ObjectDisposedException("MeasurementHandler");

			try {
				raw = JsonConvert.DeserializeObject<RawMeasurement>(message);

				if(raw.CreatedById == null)
					return;

				sensor = await this.sensors.GetAsync(raw.CreatedById).AwaitSafely();
				await this.measurements.ReceiveMeasurementAsync(sensor, raw);

				await this.stats.IncrementAsync(sensor);
			} catch(Exception ex) {
				Console.WriteLine($"Error: {ex.Message}");
				Console.WriteLine($"Received a buggy MQTT message: {message}");
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if(this.disposed)
				return;

			if(disposing) {
#if DEBUG
				this.measurements.MeasurementReceived -= this.MeasurementReceived_DebugHandler;
#endif
				this.measurements.MeasurementReceived -= this.MeasurementReceived_Handler;
			}

			this.disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
