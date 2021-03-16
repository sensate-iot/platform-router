using System.Collections.Generic;
using System.Linq;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Memory;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.TriggerService.Abstract;

namespace SensateIoT.Platform.Network.TriggerService.Caching
{
	public class TriggerActionCache : ITriggerActionCache
	{
		private readonly IMemoryCache<ObjectId, List<TriggerAction>> m_cache;

		public TriggerActionCache()
		{
			this.m_cache = new MemoryCache<ObjectId, List<TriggerAction>>();
		}

		public void Load(IEnumerable<TriggerAction> actions)
		{
			var dict = actions.GroupBy(x => x.SensorID)
				.ToDictionary(x => x.Key, g => g.ToList());

			foreach(var (key, value) in dict) {
				this.m_cache.AddOrUpdate(key, value);
			}

			this.m_cache.ScanForExpiredItems();
		}

		public void FlushSensor(ObjectId sensorId)
		{
			this.m_cache.TryRemove(sensorId);
		}

		public List<TriggerAction> Lookup(ObjectId sensorId)
		{
			List<TriggerAction> result;

			try {
				result = this.m_cache[sensorId];
			} catch(KeyNotFoundException) {
				result = null;
			}

			return result;
		}
	}
}
