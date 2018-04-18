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
using SensateService.Infrastructure.Repositories;
using SensateService.Middleware;
using SensateService.Models;
using SensateService.Models.Json.In;
using SensateService.Helpers;

namespace SensateService
{
	public class MqttMeasurementHandler : MqttHandler
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
		}

		public override void OnMessage(string topic, string msg)
		{
			throw new NotImplementedException();
		}

		public override async Task OnMessageAsync(string topic, string message)
		{
			Sensor sensor;
			RawMeasurement raw;
			AuditLog log;
			Task[] tasks;

			try {
				raw = JsonConvert.DeserializeObject<RawMeasurement>(message);

				if(raw.CreatedById == null)
					return;

				tasks = new Task[3];
				sensor = await this.sensors.GetAsync(raw.CreatedById).AwaitSafely();
				tasks[0] = this.measurements.ReceiveMeasurement(sensor, raw);

				var user = this.users.Get(sensor.Owner);
				log = new AuditLog {
					Address = IPAddress.Any,
					Method = RequestMethod.MqttTcp,
					Route = topic,
					Timestamp = DateTime.Now,
					Author = user
				};

				tasks[1] = this.auditlogs.CreateAsync(log);
				tasks[2] = this.stats.IncrementAsync(sensor);
				await Task.WhenAll(tasks);
			} catch(Exception ex) {
				Debug.WriteLine($"Error: {ex.Message}");
				Debug.WriteLine($"Received a buggy MQTT message: {message}");
			}
		}
	}
}
