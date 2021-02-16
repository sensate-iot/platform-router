/*
 * Convert messages to and from database measurement models.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;

using SensateIoT.Platform.Network.Contracts.DTO;

using DataPoint = SensateIoT.Platform.Network.Data.Models.DataPoint;
using Measurement = SensateIoT.Platform.Network.Data.Models.Measurement;

namespace SensateIoT.Platform.Network.Common.Converters
{
	public static class MeasurementDatabaseConverter
	{
		public static IDictionary<ObjectId, List<Measurement>> Convert(MeasurementData measurements)
		{
			var results = new Dictionary<ObjectId, List<Measurement>>();

			foreach(var measurement in measurements.Measurements) {
				if(!ObjectId.TryParse(measurement.SensorID, out var id)) {
					continue;
				}

				var m = new Measurement {
					Location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
						new GeoJson2DGeographicCoordinates(measurement.Longitude, measurement.Latitude)
					),
					PlatformTime = measurement.PlatformTime.ToDateTime(),
					Timestamp = measurement.Timestamp.ToDateTime(),
					Data = measurement.Datapoints.ToDictionary(x => x.Key, x => new DataPoint {
						Value = System.Convert.ToDecimal(x.Value),
						Accuracy = x.Accuracy,
						Precision = x.Precision,
						Unit = x.Unit
					}),
				};

				results.TryAdd(id, new List<Measurement>());
				results[id].Add(m);
			}

			return results;
		}
	}
}
