/*
 * MQTT message handler.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Middleware
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
			throw new System.NotImplementedException();
		}

		public override async Task OnMessageAsync(string topic, string message)
		{
			Sensor sensor;
			dynamic obj;
			string id;

			try {
				obj = JObject.Parse(message);
				id = obj.CreatedById;

				if(id == null)
					return;

				sensor = await this.sensors.GetAsync(id);
				await this.measurements.ReceiveMeasurement(sensor, message);
			} catch(Exception ex) {
				Debug.WriteLine($"Error: {ex.Message}");
				Debug.WriteLine($"Received a buggy MQTT message: {message}");
			}
		}
	}
}
