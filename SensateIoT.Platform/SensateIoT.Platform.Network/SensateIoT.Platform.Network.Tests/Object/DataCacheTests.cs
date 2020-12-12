/*
 * Unit tests for the data cache.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MongoDB.Bson;
using Moq;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Object;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Tests.Object
{
	[TestClass]
	public class DataCacheTests
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
			Assert.IsNotNull(cache.GetSensor(this.m_goodSensorID));
		}

		[TestMethod]
		public void CannotGetSensorFromBannedUser()
		{
			using var cache = this.buildCache();

			this.AddBannedUser(cache);
			Assert.IsNull(cache.GetSensor(this.m_bannedUserSensorID));
		}

		[TestMethod]
		public void CannotGetSensorFromBillingLockedUser()
		{
			using var cache = this.buildCache();

			this.AddBillingLockedUser(cache);
			Assert.IsNull(cache.GetSensor(this.m_billingLockedSensorID));
		}

		[TestMethod]
		public void CannotGetReadonlySensor()
		{
			using var cache = this.buildCache();

			this.AddReadOnlySensor(cache);
			Assert.IsNull(cache.GetSensor(this.m_readOnlySensorID));
		}

		[TestMethod]
		public void CannotGetRevokedSensor()
		{
			using var cache = this.buildCache();

			this.AddRevokedSensor(cache);
			Assert.IsNull(cache.GetSensor(this.m_revokedSensorID));
		}

		private IDataCache buildCache()
		{
			var logger = new Mock<ILogger<DataCache>>();
			var options = Options.Create(new DataCacheOptions { Timeout = TimeSpan.FromMinutes(1) });
			var cache = new DataCache(options, logger.Object);

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

		private void AddBannedUser(IDataCache cache)
		{
			var accountID = Guid.NewGuid();
			var sensorID = ObjectId.GenerateNewId();
			var apiKey = $"BannedKey";
			this.m_bannedUserSensorID = sensorID;

			var sensor = new Sensor {
				SensorKey = apiKey,
				AccountID = accountID,
				ID = sensorID,
				TriggerInformation = new SensorTrigger {
					HasActions = true
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
				Key = apiKey
			};

			cache.Append(sensor);
			cache.Append(account);
			cache.Append(key);
		}

		private void AddBillingLockedUser(IDataCache cache)
		{
			var accountID = Guid.NewGuid();
			var sensorID = ObjectId.GenerateNewId();
			var apiKey = $"BillingLocked";
			this.m_billingLockedSensorID = sensorID;

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
				HasBillingLockout = true,
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

		private void AddReadOnlySensor(IDataCache cache)
		{
			var accountID = Guid.NewGuid();
			var sensorID = ObjectId.GenerateNewId();
			var apiKey = $"ReadOnlyApiKey";
			this.m_readOnlySensorID = sensorID;

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
				HasBillingLockout = true,
				ID = accountID
			};

			var key = new ApiKey {
				IsReadOnly = true,
				IsRevoked = false,
				AccountID = accountID,
				Key = apiKey
			};

			cache.Append(sensor);
			cache.Append(account);
			cache.Append(key);
		}

		private void AddRevokedSensor(IDataCache cache)
		{
			var accountID = Guid.NewGuid();
			var sensorID = ObjectId.GenerateNewId();
			var apiKey = $"RevokedKey";
			this.m_revokedSensorID = sensorID;

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
				IsRevoked = true,
				AccountID = accountID,
				Key = apiKey
			};

			cache.Append(sensor);
			cache.Append(account);
			cache.Append(key);
		}
	}
}