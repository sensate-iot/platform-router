/*
 * Unit tests for the data cache.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using Moq;
using SensateIoT.Platform.Network.Common.Caching.Object;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Tests.Object
{
	[TestClass]
	public class DataCacheTests
	{
		private ObjectId m_goodSensorID;

		[TestMethod]
		public void CanGetAValidSensor()
		{
			var cache = this.buildCache();

			Assert.IsNotNull(cache.GetSensor(this.m_goodSensorID));
		}

		private IDataCache buildCache()
		{
			var logger = new Mock<ILogger<DataCache>>();
			var cache = new DataCache(logger.Object);

			for(var idx = 0; idx < 10; idx++) {
				var accountID = Guid.NewGuid();
				var sensorID = ObjectId.GenerateNewId();
				var apiKey = $"key::{idx}";
				this.m_goodSensorID = sensorID;

				var sensor = new Sensor {
					SensorKey = apiKey,
					AccountID = accountID,
					ID = sensorID,
					TriggerInformation = new SensorTrigger {
						HasActions = true
					}
				};

				var account = new Account {
					IsBanned = false,
					HasBillingLockout = false,
					ID = accountID
				};

				var key = new ApiKey {
					IsReadOnly = false,
					IsRevoked = false,
					AccountID = accountID,
					Key = apiKey
				};

				cache.Append(sensor);
				cache.Append(account);
				cache.Append(key);
			}

			return cache;
		}
	}
}