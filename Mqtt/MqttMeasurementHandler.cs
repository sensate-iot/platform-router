/*
 * MQTT message handler.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Newtonsoft.Json;

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

		public MqttMeasurementHandler(ISensorRepository sensors, IMeasurementRepository measurements)
		{
			this.sensors = sensors;
			this.measurements = measurements;
		}

		public override void OnMessage(string topic, string msg)
		{
			throw new NotImplementedException();
		}

		public override async Task OnMessageAsync(string topic, string message)
		{
			Sensor sensor;
			RawMeasurement raw;

			try {
				raw = JsonConvert.DeserializeObject<RawMeasurement>(message);

				if(raw.CreatedById == null)
					return;

				sensor = await this.sensors.GetAsync(raw.CreatedById);
				await this.measurements.ReceiveMeasurement(sensor, raw);
			} catch(Exception ex) {
				Debug.WriteLine($"Error: {ex.Message}");
				Debug.WriteLine($"Received a buggy MQTT message: {message}");
			}
		}
	}
}
