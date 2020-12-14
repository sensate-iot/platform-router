/*
 * Measurement and data point generation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;

using Google.Protobuf.WellKnownTypes;

using SensateIoT.Platform.Network.Contracts.DTO;

namespace SensateIoT.Platform.Network.LoadTest.RouterTest
{
	public class MeasurementGenerator
	{
		private readonly Random m_random;
		private readonly IList<string> m_sensors;

		public MeasurementGenerator(IList<string> sensors)
		{
			this.m_random = new Random();
			this.m_sensors = sensors;
		}

		private DataPoint GenerateDataPoint(string key)
		{
			var dp = new DataPoint {
				Accuracy = 0.95,
				Key = key,
				Precision = 0.5,
				Unit = "m/s2",
				Value = this.m_random.NextDouble()
			};

			return dp;
		}

		private Measurement GenerateMeasurement()
		{
			var nxt = this.m_random.Next(this.m_sensors.Count);
			var sensor = this.m_sensors[nxt];

			return new Measurement {
				Latitude = 52.6511,
				Longitude = 57.643,
				SensorID = sensor,
				Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
				Datapoints = { this.GenerateDataPoint("x"), this.GenerateDataPoint("y"), this.GenerateDataPoint("z") }
			};
		}

		public MeasurementData GenerateMeasurementData(int count)
		{
			var data = new MeasurementData();

			for(var idx = 0; idx < count; idx++) {
				data.Measurements.Add(this.GenerateMeasurement());
			}

			return data;
		}
	}
}
