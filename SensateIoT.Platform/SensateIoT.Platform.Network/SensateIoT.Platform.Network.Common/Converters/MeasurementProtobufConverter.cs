/*
 * Convert measurements to and from protobuf format.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using SensateIoT.Platform.Network.Contracts.DTO;
using Measurement = SensateIoT.Platform.Network.Data.DTO.Measurement;

namespace SensateIoT.Platform.Network.Common.Converters
{
	public class MeasurementProtobufConverter
	{
		public MeasurementData Convert(IEnumerable<Measurement> measurements)
		{
			var data = new MeasurementData();

			foreach(var measurement in measurements) {
				var m = new Contracts.DTO.Measurement {
					Latitude = System.Convert.ToDouble(measurement.Latitude),
					Longitude = System.Convert.ToDouble(measurement.Longitude),
					PlatformTime = Timestamp.FromDateTime(measurement.PlatformTimestamp),
					Timestamp = Timestamp.FromDateTime(measurement.Timestamp),
					SensorID = measurement.SensorID.ToString(),
				};

				foreach(var (key, dp) in measurement.Data) {
					m.Datapoints.Add(new DataPoint {
						Key = key,
						Accuracy = dp.Accuracy ?? 0,
						Precision = dp.Precision ?? 0,
						Unit = dp.Unit,
						Value = System.Convert.ToDouble(dp.Value)
					});
				}
			}

			return data;
		}
	}
}
