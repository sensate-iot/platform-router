/*
 * Unit tests for the data cache.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MongoDB.Bson;
using Moq;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Routing;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Tests.Cache
{
	[TestClass]
	public class RoutingCacheLookupTests
	{
		private ObjectId m_goodSensorID;
		private ObjectId m_bannedUserSensorID;
		private ObjectId m_billingLockedSensorID;
		private ObjectId m_readOnlySensorID;
		private ObjectId m_revokedSensorID;

		[TestMethod]
		public void CanGetAValidSensor()
		{
			using var cache = this.buildCache();
			Assert.IsNotNull(cache[this.m_goodSensorID]);
		}

		[TestMethod]
		public void CannotLookupWithInvalidID()
		{
			var cache = this.buildCache();
			Assert.ThrowsException<ArgumentException>(() => cache[ObjectId.Empty]);
		}


		[TestMethod]
		public void ReturnsNullWhenNotFound()
		{
			using var cache = this.buildCache();
			Assert.IsNull(cache[ObjectId.GenerateNewId()]);
		}

		[TestMethod]
		public void CannotGetSensorFromBannedUser()
		{
			using var cache = this.buildCache();

			this.AddBannedUser(cache);
			Assert.IsNull(cache[this.m_bannedUserSensorID]);
		}

		[TestMethod]
		public void CannotGetSensorFromBillingLockedUser()
		{
			using var cache = this.buildCache();

			this.AddBillingLockedUser(cache);
			Assert.IsNull(cache[this.m_billingLockedSensorID]);
		}

		[TestMethod]
		public void CannotGetReadonlySensor()
		{
			using var cache = this.buildCache();

			this.AddReadOnlySensor(cache);
			var s = cache[this.m_readOnlySensorID];
			Assert.IsNull(s);
		}

		[TestMethod]
		public void CannotGetRevokedSensor()
		{
			using var cache = this.buildCache();

			this.AddRevokedSensor(cache);
			Assert.IsNull(cache[this.m_revokedSensorID]);
		}

		[TestMethod]
		public void CannotGetSensorWithMissingAccount()
		{
			using var cache = this.buildCache();
			var sensor = cache[this.m_goodSensorID];

			sensor.AccountID = Guid.NewGuid();


			Assert.IsNull(cache[this.m_goodSensorID]);
		}

		[TestMethod]
		public void CannotGetSensorWithMissingApiKey()
		{
			using var cache = this.buildCache();
			var sensor = cache[this.m_goodSensorID];

			sensor.SensorKey = "abc";
			Assert.IsNull(cache[this.m_goodSensorID]);
		}

		private IRoutingCache buildCache()
		{
			var logger = new Mock<ILogger<RoutingCache>>();
			var cache = new RoutingCache(logger.Object);

			for(var idx = 0; idx < 10; idx++) {
				var accountID = Guid.NewGuid();
				var sensorID = ObjectId.GenerateNewId();
				var apiKey = $"key::{idx}";
				this.m_goodSensorID = sensorID;

				var sensor = new Sensor {
					SensorKey = apiKey,
					AccountID = accountID,
					ID = sensorID,
					TriggerInformation = new List<SensorTrigger> {
						new SensorTrigger {
							HasActions = true,
							IsTextTrigger = false
						}
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
				};

				cache[sensor.ID] = sensor;
				cache.Append(account);
				cache.Append(apiKey, key);
			}

			return cache;
		}

		private void AddBannedUser(IRoutingCache cache)
		{
			var accountID = Guid.NewGuid();
			var sensorID = ObjectId.GenerateNewId();
			var apiKey = $"BannedKey";
			this.m_bannedUserSensorID = sensorID;

			var sensor = new Sensor {
				SensorKey = apiKey,
				AccountID = accountID,
				ID = sensorID,
				TriggerInformation = new List<SensorTrigger> {
					new SensorTrigger {
						HasActions = true,
						IsTextTrigger = false
					}
				}
			};

			var account = new Account {
				IsBanned = true,
				HasBillingLockout = false,
				ID = accountID
			};

			var key = new ApiKey {
				IsReadOnly = false,
				IsRevoked = false,
				AccountID = accountID,
			};

			cache[sensor.ID] = sensor;
			cache.Append(account);
			cache.Append(apiKey, key);
		}

		private void AddBillingLockedUser(IRoutingCache cache)
		{
			var accountID = Guid.NewGuid();
			var sensorID = ObjectId.GenerateNewId();
			var apiKey = "BillingLocked";
			this.m_billingLockedSensorID = sensorID;

			var sensor = new Sensor {
				SensorKey = apiKey,
				AccountID = accountID,
				ID = sensorID,
				TriggerInformation = new List<SensorTrigger> {
					new SensorTrigger {
						HasActions = true,
						IsTextTrigger = false
					}
				}
			};

			var account = new Account {
				IsBanned = false,
				HasBillingLockout = true,
				ID = accountID
			};

			var key = new ApiKey {
				IsReadOnly = false,
				IsRevoked = false,
				AccountID = accountID,
			};

			cache[sensor.ID] = sensor;
			cache.Append(account);
			cache.Append(apiKey, key);
		}

		private void AddReadOnlySensor(IRoutingCache cache)
		{
			var accountID = Guid.NewGuid();
			var sensorID = ObjectId.GenerateNewId();
			var apiKey = "ReadOnlyApiKey";
			this.m_readOnlySensorID = sensorID;

			var sensor = new Sensor {
				SensorKey = apiKey,
				AccountID = accountID,
				ID = sensorID,
				TriggerInformation = new List<SensorTrigger> {
					new SensorTrigger {
						HasActions = true,
						IsTextTrigger = false
					}
				}
			};

			var account = new Account {
				IsBanned = false,
				HasBillingLockout = true,
				ID = accountID
			};

			var key = new ApiKey {
				IsReadOnly = true,
				IsRevoked = false,
				AccountID = accountID,
			};

			cache[sensor.ID] = sensor;
			cache.Append(account);
			cache.Append(apiKey, key);
		}

		private void AddRevokedSensor(IRoutingCache cache)
		{
			var accountID = Guid.NewGuid();
			var sensorID = ObjectId.GenerateNewId();
			var apiKey = $"RevokedKey";
			this.m_revokedSensorID = sensorID;

			var sensor = new Sensor {
				SensorKey = apiKey,
				AccountID = accountID,
				ID = sensorID,
				TriggerInformation = new List<SensorTrigger> {
					new SensorTrigger {
						HasActions = true,
						IsTextTrigger = false
					}
				}
			};

			var account = new Account {
				IsBanned = false,
				HasBillingLockout = false,
				ID = accountID
			};

			var key = new ApiKey {
				IsReadOnly = false,
				IsRevoked = true,
				AccountID = accountID,
			};

			cache[sensor.ID] = sensor;
			cache.Append(account);
			cache.Append(apiKey, key);
		}
	}
}