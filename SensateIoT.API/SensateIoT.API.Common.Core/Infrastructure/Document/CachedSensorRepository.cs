/*
 * Cached sensor repository.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SensateIoT.API.Common.Core.Helpers;
using SensateIoT.API.Common.Core.Infrastructure.Cache;
using SensateIoT.API.Common.Data.Enums;
using SensateIoT.API.Common.Data.Models;
using SensateIoT.API.Common.IdentityData.Models;

namespace SensateIoT.API.Common.Core.Infrastructure.Document
{
	public class CachedSensorRepository : SensorRepository
	{
		private readonly ICacheStrategy<string> _cache;

		public CachedSensorRepository(SensateContext context, ICacheStrategy<string> cache) :
			base(context)
		{
			this._cache = cache;
		}

		private Task CommitAsync(Sensor obj, int tmo = 10, CancellationToken ct = default)
		{
			var str = JsonConvert.SerializeObject(obj);
			return this._cache.SetAsync(obj.InternalId.ToString(), str, tmo, ct);
		}

		public override async Task<IEnumerable<Sensor>> GetAsync(SensateUser user, int skip = 0, int limit = 0)
		{
			string key, data;
			IEnumerable<Sensor> sensors;

			key = $"sensors:uuid:{user.Id}:{skip}:{limit}";
			data = await this._cache.GetAsync(key);

			if(data != null) {
				return JsonConvert.DeserializeObject<IEnumerable<Sensor>>(data);
			}

			sensors = await base.GetAsync(user, skip, limit).AwaitBackground();

			if(sensors == null) {
				return null;
			}

			await this._cache.SetAsync(key, JsonConvert.SerializeObject(sensors), CacheTimeout.TimeoutMedium.ToInt()).AwaitBackground();
			return sensors;
		}

		public override async Task<Sensor> GetAsync(string id)
		{
			string data;
			Sensor sensor;

			data = await this._cache.GetAsync(id).AwaitBackground();

			if(data != null) {
				return JsonConvert.DeserializeObject<Sensor>(data);
			}

			sensor = await base.GetAsync(id).AwaitBackground();

			if(sensor == null) {
				return null;
			}

			await this.CommitAsync(sensor, CacheTimeout.TimeoutMedium.ToInt()).AwaitBackground();
			return sensor;
		}
	}
}
