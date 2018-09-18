/*
 * MQTT message handler.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Diagnostics;
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
	public class MqttMeasurementHandler : Middleware.MqttHandler
	{
		private readonly ISensorRepository sensors;
		private readonly IMeasurementRepository measurements;
		private readonly IAuditLogRepository auditlogs;
		private readonly IUserRepository users;
		private readonly ISensorStatisticsRepository stats;

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

			MeasurementEvents.MeasurementReceived += this.MeasurementReceived_Handler;
		}

		private async Task MeasurementReceived_Handler(object sender, MeasurementReceivedEventArgs e)
		{
			SensateUser user;
			AuditLog log;

			if(!(sender is Sensor sensor))
				return;

			user = this.users.Get(sensor.Owner);
			log = new AuditLog {
				Address = IPAddress.Any,
				Method = RequestMethod.MqttTcp,
				Route = "NA",
				Timestamp = DateTime.Now,
				Author = user
			};

			await this.auditlogs.CreateAsync(log);
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
			Task[] tasks;

			try {
				raw = JsonConvert.DeserializeObject<RawMeasurement>(message);

				if(raw.CreatedById == null)
					return;

				tasks = new Task[2];
				sensor = await this.sensors.GetAsync(raw.CreatedById).AwaitSafely();
				tasks[0] = this.measurements.ReceiveMeasurement(sensor, raw);
				tasks[1] = this.stats.IncrementAsync(sensor);

				await Task.WhenAll(tasks).AwaitSafely();
			} catch(Exception ex) {
				Debug.WriteLine($"Error: {ex.Message}");
				Debug.WriteLine($"Received a buggy MQTT message: {message}");
			}
		}
	}
}
