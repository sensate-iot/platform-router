/*
 * Convert measurements to and from protobuf format.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;

using Google.Protobuf.WellKnownTypes;
using MongoDB.Bson;

using SensateIoT.Platform.Router.Contracts.DTO;

using Measurement = SensateIoT.Platform.Router.Data.DTO.Measurement;

namespace SensateIoT.Platform.Router.Common.Converters
{
	public static class MeasurementProtobufConverter
	{
		public static Contracts.DTO.Measurement Convert(Measurement measurement)
		{
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

			return m;
		}

		public static MeasurementData Convert(IEnumerable<Measurement> measurements)
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

				data.Measurements.Add(m);
			}

			return data;
		}

		public static Measurement Convert(Contracts.DTO.Measurement measurement)
		{
			return new Measurement {
				Data = measurement.Datapoints.ToDictionary(x => x.Key, x => new Data.DTO.DataPoint {
					Value = System.Convert.ToDecimal(x.Value),
					Accuracy = x.Accuracy,
					Precision = x.Precision,
					Unit = x.Unit
				}),
				Latitude = System.Convert.ToDecimal(measurement.Latitude),
				Longitude = System.Convert.ToDecimal(measurement.Longitude),
				SensorId = ObjectId.Parse(measurement.SensorID),
				Timestamp = measurement.Timestamp == null ? DateTime.UtcNow : measurement.Timestamp.ToDateTime(),
				PlatformTimestamp = measurement.PlatformTime == null ? DateTime.UtcNow : measurement.PlatformTime.ToDateTime()
			};
		}
	}
}
