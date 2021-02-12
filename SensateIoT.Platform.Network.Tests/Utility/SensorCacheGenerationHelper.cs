/*
 * Sensor cache builder/generation helper.
 */

using System;
using System.Collections.Generic;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Object;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Tests.Utility
{
	public class SensorCacheGenerationHelper
	{
		public static Tuple<List<ObjectId>, ISensorCache> BuildSensors()
		{
			var cache = new SensorCache(100, TimeSpan.FromMinutes(5));
			var list = new List<ObjectId>();
			var opts = new CacheEntryOptions {
				Size = 1
			};

			for(var idx = 0; idx < 10; idx++) {
				var sensor = new Sensor {
					LiveDataRouting = null,
					TriggerInformation = default,
					AccountID = Guid.NewGuid(),
					SensorKey = Guid.NewGuid().ToString(),
					ID = ObjectId.GenerateNewId()
				};

				list.Add(sensor.ID);
				cache.Add(sensor.ID, sensor, opts);
			}

			return new Tuple<List<ObjectId>, ISensorCache>(list, cache);
		}

		public static IList<LiveDataRoute> GenerateRoutes(IList<ObjectId> objectIds)
		{
			return new List<LiveDataRoute> {
				new LiveDataRoute {
					SensorID = objectIds[0],
					Target = "s1"
				},
				new LiveDataRoute {
					SensorID = objectIds[0],
					Target = "s1"
				},
				new LiveDataRoute {
					SensorID = objectIds[2],
					Target = "s1"
				}
			};
		}
	}
}
