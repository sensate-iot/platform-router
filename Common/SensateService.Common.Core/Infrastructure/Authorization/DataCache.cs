/*
 * Data cache for sensors, users and API keys.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MongoDB.Bson;

using SensateService.Common.Caching.Abstract;
using SensateService.Common.Caching.Memory;
using SensateService.Common.Data.Dto.Authorization;
using SensateService.Helpers;

namespace SensateService.Infrastructure.Authorization
{
	public class DataCache : IDataCache
	{
		private readonly IMemoryCache<ObjectId, Sensor> m_sensors;
		private readonly IMemoryCache<string, User> m_users;
		private readonly IMemoryCache<string, ApiKey> m_apiKeys;

		private const long CacheCapacity = -1L;
		private const int TimeoutMinutes = 6;

		public DataCache()
		{
			var tmo = Convert.ToInt32(TimeSpan.FromMinutes(TimeoutMinutes).TotalMilliseconds);

			this.m_sensors = new MemoryCache<ObjectId, Sensor>(CacheCapacity, tmo);
			this.m_users = new MemoryCache<string, User>(CacheCapacity, tmo);
			this.m_apiKeys = new MemoryCache<string, ApiKey>(CacheCapacity, tmo);
		}

		public Sensor GetSensor(ObjectId id)
		{
			Sensor s;

			try {
				s = this.m_sensors[id];
				var key = this.m_apiKeys[s.Secret];
				var user = this.m_users[s.UserId];

				if(user.Banned || user.BillingLockout || key.ReadOnly || key.ReadOnly) {
					return null;
				}
			} catch(ArgumentOutOfRangeException) {
				s = null;
			}

			return s;
		}

		public void Append(IEnumerable<Sensor> sensors)
		{
			var sensorsKvp = sensors.Select(s => new Common.Caching.Abstract.KeyValuePair<ObjectId, Sensor> {
				Value = s,
				Key = s.Id
			});

			this.m_sensors.AddOrUpdate(sensorsKvp);
		}

		public void Append(IEnumerable<User> users)
		{
			var usersKvp = users.Select(u => new Common.Caching.Abstract.KeyValuePair<string, User> {
				Value = u,
				Key = u.Id
			});

			this.m_users.AddOrUpdate(usersKvp);
		}

		public void Append(IEnumerable<ApiKey> keys)
		{
			var keysKvp = keys.Select(key => new Common.Caching.Abstract.KeyValuePair<string, ApiKey> {
				Key = key.Key,
				Value = key
			});

			this.m_apiKeys.AddOrUpdate(keysKvp);
		}

		public async Task Clear()
		{
			var tsk = Task.Run(() => {
				this.m_sensors.ScanForExpiredItems();
				this.m_users.ScanForExpiredItems();
				this.m_apiKeys.ScanForExpiredItems();
			});

			await tsk.AwaitBackground();

			tsk = Task.Run(() => {
				this.m_users.RemoveScheduledEntries();
				this.m_sensors.RemoveScheduledEntries();
				this.m_apiKeys.RemoveScheduledEntries();
			});

			await tsk.AwaitBackground();
		}
	}
}
