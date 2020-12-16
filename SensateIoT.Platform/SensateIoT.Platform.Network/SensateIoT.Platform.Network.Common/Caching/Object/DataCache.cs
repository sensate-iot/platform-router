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
using Microsoft.Extensions.Options;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Memory;
using SensateIoT.Platform.Network.Common.Exceptions;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.Data.Models;

using ApiKey = SensateIoT.Platform.Network.Data.DTO.ApiKey;
using Sensor = SensateIoT.Platform.Network.Data.DTO.Sensor;

namespace SensateIoT.Platform.Network.Common.Caching.Object
{
	public sealed class DataCache : IDataCache
	{
		private readonly ISensorCache m_sensors;
		private readonly IMemoryCache<Guid, Account> m_accounts;
		private readonly IMemoryCache<string, ApiKey> m_keys;
		private readonly DataCacheOptions _mOptions;
		private readonly ILogger<DataCache> m_logger;
		private readonly IMemoryCache<string, LiveDataHandler> m_remotes;

		public DataCache(IOptions<DataCacheOptions> options, ILogger<DataCache> logger)
		{
			var tmo = options.Value.Timeout;
			var capacity = options.Value.Capacity ?? MemoryCache<int, int>.DefaultCapacity;
			this._mOptions = options.Value;

			this.m_sensors = new SensorCache(capacity, tmo);
			this.m_keys = new MemoryCache<string, ApiKey>(capacity, tmo);
			this.m_accounts = new MemoryCache<Guid, Account>(capacity, tmo);
			this.m_remotes = new MemoryCache<string, LiveDataHandler>(MemoryCache<string, LiveDataHandler>.DefaultCapacity, tmo);
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
				this.m_logger.LogError("Unable to retrieve cache value for key: {id}. " +
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

			try {
				this.m_sensors.AddOrUpdate(sensorsKvp, new CacheEntryOptions { Size = CalculateEntrySize(this._mOptions) });
			} catch(ArgumentOutOfRangeException ex) {
				this.m_logger.LogError("Unable to update cache: {message}.", ex.Message);
			}
		}

		public void Append(IEnumerable<Account> accounts)
		{
			var accountsKvp = accounts.Select(a => new Abstract.KeyValuePair<Guid, Account> {
				Key = a.ID,
				Value = a
			});

			try {
				this.m_accounts.AddOrUpdate(accountsKvp, new CacheEntryOptions { Size = CalculateEntrySize(this._mOptions) });
			} catch(ArgumentOutOfRangeException ex) {
				this.m_logger.LogError("Unable to update cache: {message}.", ex.Message);
			}
		}

		public void Append(IEnumerable<ApiKey> keys)
		{
			var keysKvp = keys.Select(k => new Abstract.KeyValuePair<string, ApiKey> {
				Value = k,
				Key = k.Key
			});

			try {
				this.m_keys.AddOrUpdate(keysKvp, new CacheEntryOptions { Size = CalculateEntrySize(this._mOptions) });
			} catch(ArgumentOutOfRangeException ex) {
				this.m_logger.LogError("Unable to add key to cache: {message}.", ex.Message);
			}
		}

		public void Append(Sensor sensor)
		{
			try {
				this.m_sensors.AddOrUpdate(sensor.ID, sensor, new CacheEntryOptions { Size = CalculateEntrySize(this._mOptions) });
			} catch(ArgumentOutOfRangeException ex) {
				this.m_logger.LogError("Unable to update cache: {message}.", ex.Message);
			}
		}

		public void Append(Account account)
		{
			try {
				this.m_accounts.AddOrUpdate(account.ID, account, new CacheEntryOptions { Size = CalculateEntrySize(this._mOptions) });
			} catch(ArgumentOutOfRangeException ex) {
				this.m_logger.LogError("Unable to update cache: {message}.", ex.Message);
			}
		}

		public void Append(ApiKey key)
		{
			try {
				this.m_keys.AddOrUpdate(key.Key, key, new CacheEntryOptions { Size = CalculateEntrySize(this._mOptions) });
			} catch(ArgumentOutOfRangeException ex) {
				this.m_logger.LogError("Unable to update cache: {message}.", ex.Message);
			}
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

		public void AddLiveDataRoute(LiveDataRoute route)
		{
			if(!this.m_remotes.TryGetValue(route.Target, out var remote) || !remote.Enabled) {
				return;
			}

			this.m_sensors.AddLiveDataRouting(route);
		}

		public void RemoveLiveDataRoute(LiveDataRoute route)
		{
			this.m_sensors.RemoveLiveDataRouting(route);
		}

		public void SyncLiveData(ICollection<LiveDataRoute> data)
		{
			var list = data.ToList();

			list.RemoveAll(x => !this.m_remotes.TryGetValue(x.Target, out var value) || !value.Enabled);
			this.m_sensors.SyncLiveDataRoutes(list);
		}

		public void SetLiveDataRemotes(IEnumerable<LiveDataHandler> remotes)
		{
			var kvp = remotes.Select(x => new Abstract.KeyValuePair<string, LiveDataHandler> {
				Value = x,
				Key = x.Name
			});

			this.m_remotes.AddOrUpdate(kvp);
		}

		public void FlushLiveData()
		{
			this.m_sensors.FlushLiveDataRoutes();
		}

		private static long? CalculateEntrySize(DataCacheOptions cache)
		{
			long? size = null;

			if(cache.Capacity != null) {
				size = 1;
			}

			return size;
		}

		public void Dispose()
		{
			this.Clear();
			this.m_sensors.Dispose();
			this.m_accounts.Dispose();
			this.m_keys.Dispose();

			GC.SuppressFinalize(this);
		}
	}
}
