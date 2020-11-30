/*
 *
 */

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.Tests.Utility;

namespace SensateIoT.Platform.Network.Tests.Object
{
	[TestClass]
	public class SensorCacheTests
	{
		[TestMethod]
		public void CanSyncLiveDataRoutesRoutes()
		{
			var cache = SensorCacheGenerationHelper.BuildSensors();
			var routes = SensorCacheGenerationHelper.GenerateRoutes(cache.Item1);

			cache.Item2.SyncLiveDataRoutes(routes);
			cache.Item2.FlushLiveDataRoutes();
			var sensor = cache.Item2[cache.Item1[0]];
			Assert.AreEqual(1, sensor.LiveDataRouting.Count);
			Assert.AreEqual("s1", sensor.LiveDataRouting.First().Target);
		}

		[TestMethod]
		public void SyncOverwritesCurrentRoutes()
		{
			var cache = SensorCacheGenerationHelper.BuildSensors();
			var routes = SensorCacheGenerationHelper.GenerateRoutes(cache.Item1);

			cache.Item2.SyncLiveDataRoutes(routes);
			cache.Item2.FlushLiveDataRoutes();
			var sensor = cache.Item2[cache.Item1[0]];

			Assert.AreEqual(1, sensor.LiveDataRouting.Count);

			foreach(var route in routes) {
				route.Target = "s2";
			}

			cache.Item2.SyncLiveDataRoutes(routes);
			cache.Item2.FlushLiveDataRoutes();

			sensor = cache.Item2[cache.Item1[0]];

			Assert.AreEqual(1, sensor.LiveDataRouting.Count);
			Assert.AreEqual("s2", sensor.LiveDataRouting.First().Target);
		}

		[TestMethod]
		public void CanUpdateSensors()
		{
			var cache = SensorCacheGenerationHelper.BuildSensors();
			var routes = SensorCacheGenerationHelper.GenerateRoutes(cache.Item1);

			cache.Item2.SyncLiveDataRoutes( routes);
			cache.Item2.FlushLiveDataRoutes();

			var s = cache.Item2[cache.Item1[0]];

			var sensor = new Sensor {
				ID = s.ID,
				AccountID = s.AccountID,
				SensorKey = "ABC",
				TriggerInformation = s.TriggerInformation
			};

			cache.Item2.AddOrUpdate(sensor.ID, sensor, new CacheEntryOptions { Size = 1 });

			s = cache.Item2[s.ID];
			Assert.AreEqual(1, s.LiveDataRouting.Count);
			Assert.AreEqual("s1", s.LiveDataRouting.First().Target);
			Assert.AreEqual("ABC", s.SensorKey);
		}


		[TestMethod]
		public void CanBulkUpdateSensors()
		{
			var cache = SensorCacheGenerationHelper.BuildSensors();
			var routes = SensorCacheGenerationHelper.GenerateRoutes(cache.Item1);

			cache.Item2.SyncLiveDataRoutes( routes);
			cache.Item2.FlushLiveDataRoutes();

			var list = new List<Common.Caching.Abstract.KeyValuePair<ObjectId, Sensor>>();

			foreach(var id in cache.Item1) {
				var sensor = cache.Item2[id];

				var newSensor = new Sensor {
					ID = id,
					AccountID = sensor.AccountID,
					TriggerInformation = sensor.TriggerInformation,
					SensorKey = "ABC"
				};

				list.Add(new Common.Caching.Abstract.KeyValuePair<ObjectId, Sensor> { Key = id, Value = newSensor });
			}

			cache.Item2.AddOrUpdate(list, new CacheEntryOptions { Size = 1 });
			var s = cache.Item2[cache.Item1[0]];
			Assert.AreEqual(1, s.LiveDataRouting.Count);
			Assert.AreEqual("s1", s.LiveDataRouting.First().Target);
			Assert.AreEqual("ABC", s.SensorKey);
		}

		[TestMethod]
		public void CanAddLiveDataRoutes()
		{
			var cache = SensorCacheGenerationHelper.BuildSensors();
			var route = new LiveDataRoute {
				SensorID = cache.Item1[0],
				Target = "s1"
			};

			cache.Item2.AddLiveDataRouting(route);
			var sensor = cache.Item2[cache.Item1[0]];

			Assert.AreEqual(1, sensor.LiveDataRouting.Count);
			Assert.AreEqual("s1", sensor.LiveDataRouting.First().Target);
		}

		[TestMethod]
		public void CanRemoveLiveDataRoutes()
		{
			var cache = SensorCacheGenerationHelper.BuildSensors();
			var routes = new List<LiveDataRoute> {
				new LiveDataRoute {
					SensorID = cache.Item1[0],
					Target = "s1"
				},
				new LiveDataRoute {
					SensorID = cache.Item1[0],
					Target = "s1"
				},
				new LiveDataRoute {
					SensorID = cache.Item1[2],
					Target = "s1"
				}
			};

			cache.Item2.AddLiveDataRouting(new LiveDataRoute { SensorID = cache.Item1[0], Target = "s2" });

			cache.Item2.SyncLiveDataRoutes( routes);
			cache.Item2.FlushLiveDataRoutes();
			var sensor = cache.Item2[cache.Item1[0]];

			cache.Item2.RemoveLiveDataRouting(new LiveDataRoute { SensorID = cache.Item1[0], Target = "s1" });
			Assert.AreEqual(1, sensor.LiveDataRouting.Count);
			Assert.AreEqual("s2", sensor.LiveDataRouting.First().Target);
			Assert.AreEqual("s1", cache.Item2[cache.Item1[2]].LiveDataRouting.First().Target);
		}
	}
}
