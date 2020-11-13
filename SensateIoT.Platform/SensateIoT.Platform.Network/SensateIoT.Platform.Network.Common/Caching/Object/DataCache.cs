/*
 * Object cache to store routing information related to sensors and messages.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Memory;
using SensateIoT.Platform.Network.Common.Exceptions;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Common.Caching.Object
{
	public sealed class DataCache : IDataCache
	{
		private readonly IMemoryCache<ObjectId, Sensor> m_sensors;
		private readonly IMemoryCache<Guid, Account> m_accounts;
		private readonly IMemoryCache<string, ApiKey> m_keys;
		private readonly ILogger<DataCache> m_logger;

		private const int DefaultTimeout = 6;
		private const long DefaultCapacity = -1L;

		public DataCache(ILogger<DataCache> logger)
		{
			var tmo = TimeSpan.FromMinutes(DefaultTimeout);

			this.m_sensors = new MemoryCache<ObjectId, Sensor>(DefaultCapacity, tmo);
			this.m_keys = new MemoryCache<string, ApiKey>(DefaultCapacity, tmo);
			this.m_accounts = new MemoryCache<Guid, Account>(DefaultCapacity, tmo);
			this.m_logger = logger;

			this.m_sensors.ActiveTimeoutScanningEnabled = false;
			this.m_accounts.ActiveTimeoutScanningEnabled = false;
			this.m_keys.ActiveTimeoutScanningEnabled = false;
		}

		~DataCache()
		{
			this.Dispose();
		}

		public Sensor GetSensor(ObjectId id)
		{
			Sensor sensor;

			try {
				sensor = this.m_sensors[id];
				var account = this.m_accounts[sensor.AccountID];
				var key = this.m_keys[sensor.SensorKey];

				if(account.IsBanned || account.HasBillingLockout ||
				   key.IsReadOnly || key.IsRevoked || account.ID != key.AccountID) {
					sensor = null;
				}
			} catch(KeyNotFoundException) {
				sensor = null;
			} catch(CacheException ex) {
				sensor = null;
				this.m_logger.LogWarning("Unable to retrieve cache value for key: {id}. " +
										 "Reason: {exception}. Trace: {trace}.", ex.Key, ex.Message, ex.StackTrace);
			}

			return sensor;
		}

		public void Append(IEnumerable<Sensor> sensors)
		{
			var sensorsKvp = sensors.Select(s => new Abstract.KeyValuePair<ObjectId, Sensor> {
				Key = s.ID,
				Value = s
			});

			this.m_sensors.AddOrUpdate(sensorsKvp);
		}

		public void Append(IEnumerable<Account> accounts)
		{
			var accountsKvp = accounts.Select(a => new Abstract.KeyValuePair<Guid, Account> {
				Key = a.ID,
				Value = a
			});

			this.m_accounts.AddOrUpdate(accountsKvp);
		}

		public void Append(IEnumerable<ApiKey> keys)
		{
			var keysKvp = keys.Select(k => new Abstract.KeyValuePair<string, ApiKey> {
				Value = k,
				Key = k.Key
			});

			this.m_keys.AddOrUpdate(keysKvp);
		}

		public void Append(Sensor sensor)
		{
			this.m_sensors.AddOrUpdate(sensor.ID, sensor);
		}

		public void Append(Account user)
		{
			this.m_accounts.AddOrUpdate(user.ID, user);
		}

		public void Append(ApiKey key)
		{
			this.m_keys.AddOrUpdate(key.Key, key);
		}

		public void Clear()
		{
			this.m_sensors.Clear();
			this.m_accounts.Clear();
			this.m_keys.Clear();
		}

		public void RemoveSensor(ObjectId sensorID)
		{
			try {
				this.m_sensors.TryRemove(sensorID);
			} catch(CacheException ex) {
				this.m_logger.LogWarning("Unable to remove cache value for key: {id}. " +
										 "Reason: {exception}. Trace: {trace}.", ex.Key, ex.Message, ex.StackTrace);
			}
		}

		public void RemoveAccount(Guid accountID)
		{
			try {
				this.m_accounts.TryRemove(accountID);
			} catch(CacheException ex) {
				this.m_logger.LogWarning("Unable to remove cache value for key: {id}. " +
										 "Reason: {exception}. Trace: {trace}.", ex.Key, ex.Message, ex.StackTrace);
			}
		}

		public void RemoveApiKey(string key)
		{
			try {
				this.m_keys.TryRemove(key);
			} catch(CacheException ex) {
				this.m_logger.LogWarning("Unable to remove cache value for key: {id}. " +
										 "Reason: {exception}. Trace: {trace}.", ex.Key, ex.Message, ex.StackTrace);
			}
		}

		public async Task ScanCachesAsync()
		{
			Parallel.Invoke(
				() => this.m_sensors.ScanForExpiredItems(),
				() => this.m_accounts.ScanForExpiredItems(),
				() => this.m_keys.ScanForExpiredItems()
			);

			await Task.WhenAll(
				this.m_sensors.RemoveScheduledEntriesAsync(),
				this.m_accounts.RemoveScheduledEntriesAsync(),
				this.m_keys.RemoveScheduledEntriesAsync()
			).ConfigureAwait(false);

			GarbageCollection.Collect();
		}

		public void Dispose()
		{
			this.m_sensors.Dispose();
			this.m_accounts.Dispose();
			this.m_keys.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
