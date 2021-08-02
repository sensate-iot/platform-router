/*
 * Sensor cache builder/generation helper.
 */

using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using Moq;
using MongoDB.Bson;

using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Router.Common.Caching.Abstract;
using SensateIoT.Platform.Router.Common.Caching.Routing;
using SensateIoT.Platform.Router.Data.DTO;

using Sensor = SensateIoT.Platform.Router.Data.DTO.Sensor;

namespace SensateIoT.Platform.Network.Tests.Utility
{
	public static class SensorCacheGenerationHelper
	{
		public static Tuple<List<ObjectId>, IRoutingCache> BuildSensors()
		{
			var loggerMoq = new Mock<ILogger<RoutingCache>>();
			var cache = new RoutingCache(loggerMoq.Object);
			var list = new List<ObjectId>();

			cache.SetLiveDataRemotes(new[] {
				new LiveDataHandler {
					Enabled = true,
					ID = 1,
					Name = "s1"
				},
				new LiveDataHandler {
					Enabled = true,
					ID = 1,
					Name = "s2"
				}
			});

			for(var idx = 0; idx < 10; idx++) {
				var sensor = new Sensor {
					LiveDataRouting = null,
					TriggerInformation = default,
					AccountID = Guid.NewGuid(),
					SensorKey = Guid.NewGuid().ToString(),
					ID = ObjectId.GenerateNewId()
				};

				var account = new Account {
					HasBillingLockout = false,
					ID = sensor.AccountID,
					IsBanned = false
				};

				var key = new ApiKey {
					AccountID = sensor.AccountID,
					IsReadOnly = false,
					IsRevoked = false
				};

				list.Add(sensor.ID);
				cache[sensor.ID] = sensor;
				cache.Append(account);
				cache.Append(sensor.SensorKey, key);
			}

			return new Tuple<List<ObjectId>, IRoutingCache>(list, cache);
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
