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

namespace SensateService
{
	public class MqttMeasurementHandler : MqttHandler
	{
		private readonly ISensorRepository sensors;
		private readonly IMeasurementRepository measurements;
		private readonly IAuditLogRepository auditlogs;
		private readonly IUserRepository users;
		private readonly ISensorStatistics stats;

		public MqttMeasurementHandler(ISensorRepository sensors,
									  IMeasurementRepository measurements,
									  IAuditLogRepository auditlogs,
									  ISensorStatistics stats,
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

			try {
				raw = JsonConvert.DeserializeObject<RawMeasurement>(message);

				if(raw.CreatedById == null)
					return;

				sensor = await this.sensors.GetAsync(raw.CreatedById);
				await this.measurements.ReceiveMeasurement(sensor, raw);

				var user = this.users.Get(sensor.Owner);
				log = new AuditLog {
					Address = IPAddress.Any,
					Method = RequestMethod.MqttTcp,
					Route = topic,
					Timestamp = DateTime.Now,
					Author = user
				};

				await this.auditlogs.CreateAsync(log);
				await this.stats.IncrementAsync(sensor);
			} catch(Exception ex) {
				Debug.WriteLine($"Error: {ex.Message}");
				Debug.WriteLine($"Received a buggy MQTT message: {message}");
			}
		}
	}
}
