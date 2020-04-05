/*
 * Cached sensor repository.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SensateService.Enums;
using SensateService.Infrastructure.Cache;
using SensateService.Models;
using SensateService.Helpers;

namespace SensateService.Infrastructure.Document
{
	public class CachedSensorRepository : SensorRepository
	{
		private readonly ICacheStrategy<string> _cache;

		public CachedSensorRepository(SensateContext context, ILogger<SensorRepository> logger, ICacheStrategy<string> cache) :
			base(context, logger)
		{
			this._cache = cache;
		}

		private void Commit(Sensor obj)
		{
			this._cache.Set(obj.InternalId.ToString(), obj.ToJson());
		}

		private async Task CommitAsync(Sensor obj, CancellationToken ct = default(CancellationToken))
		{
			await this._cache.SetAsync(
				obj.InternalId.ToString(),
				obj.ToJson(),
				CacheTimeout.Timeout.ToInt(),
				true,
				ct
			).AwaitBackground();
		}

		public override async Task CreateAsync(Sensor sensor, CancellationToken ct = default(CancellationToken))
		{
			var tasks = new Task[4];

			tasks[0] = base.CreateAsync(sensor, ct);
			tasks[1] = this.CommitAsync(sensor, ct);
			tasks[2] = this._cache.RemoveAsync($"sensors:uid:{sensor.Owner}");
			tasks[3] = this._cache.RemoveAsync($"sensors:uid:{sensor.Owner}:0:0");

			await Task.WhenAll(tasks).AwaitBackground();
		}

		public override void Create(Sensor obj)
		{
			base.Create(obj);
			this.Commit(obj);
		}

		public override async Task<IEnumerable<Sensor>> GetAsync(SensateUser user, int skip = 0, int limit = 0)
		{
			string key, data;
			IEnumerable<Sensor> sensors;

			key = $"sensors:uid:{user.Id}:{skip}:{limit}";
			data = await this._cache.GetAsync(key);

			if(data != null) {
				return JsonConvert.DeserializeObject<IEnumerable<Sensor>>(data);
			}

			sensors = await base.GetAsync(user, skip, limit).AwaitBackground();

			if(sensors == null) {
				return null;
			}

			await this._cache.SetAsync(key, JsonConvert.SerializeObject(sensors), CacheTimeout.TimeoutMedium.ToInt(), false).AwaitBackground();
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

			await this.CommitAsync(sensor).AwaitBackground();
			return sensor;

		}

		public override async Task UpdateSecretAsync(Sensor sensor, SensateApiKey key)
		{
			var tasks = new[] {
				this._cache.SetAsync(sensor.InternalId.ToString(), sensor.ToJson(),
									 CacheTimeout.TimeoutMedium.ToInt(), false),
				this._cache.RemoveAsync($"sensors:uid:{sensor.Owner}"),
				this._cache.RemoveAsync($"sensors:uid:{sensor.Owner}:0:0"),
				this._cache.RemoveAsync(sensor.Owner),
				base.UpdateSecretAsync(sensor, key)
			};

			await Task.WhenAll(tasks).AwaitBackground();
		}

		public override async Task UpdateAsync(Sensor sensor)
		{
			var tasks = new[] {
				this._cache.SetAsync(sensor.InternalId.ToString(), sensor.ToJson()),
				this._cache.RemoveAsync($"sensors:uid:{sensor.Owner}"),
				this._cache.RemoveAsync($"sensors:uid:{sensor.Owner}:0:0"),
				this._cache.RemoveAsync(sensor.Owner),
				base.UpdateAsync(sensor),
			};

			await Task.WhenAll(tasks).AwaitBackground();
		}

		public override async Task DeleteAsync(Sensor sensor, CancellationToken ct = default)
		{
			var tsk = new[] {
				this._cache.RemoveAsync(sensor.InternalId.ToString()),
				this._cache.RemoveAsync($"sensors:uid:{sensor.Owner}"),
				this._cache.RemoveAsync($"sensors:uid:{sensor.Owner}:0:0"),
				this._cache.RemoveAsync(sensor.InternalId.ToString()),
				base.DeleteAsync(sensor, ct)
			};

			await Task.WhenAll(tsk).AwaitBackground();
		}
	}
}
