/*
 * Cached sensor repository.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using SensateService.Infrastructure.Cache;
using SensateService.Models;
using SensateService.Infrastructure.Repositories;
using SensateService.Helpers;

namespace SensateService.Infrastructure.Document
{
	public class CachedSensorRepository : SensorRepository
	{
		private readonly ICacheStrategy<string> _cache;

		public CachedSensorRepository(
			SensateContext context,
			IMeasurementRepository measurements,
			ISensorStatisticsRepository stats,
			ILogger<SensorRepository> logger,
			ICacheStrategy<string> cache) : base(context, stats, measurements, logger)
		{
			this._cache = cache;
		}

		private void Commit(Sensor obj)
		{
			this._cache.Set(obj.InternalId.ToString(), obj.ToJson());
		}

		private async Task CommitAsync( Sensor obj, CancellationToken ct = default(CancellationToken) )
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
			var tasks = new Task[2];

			tasks[0] = base.CreateAsync(sensor, ct);
			tasks[1] = this.CommitAsync(sensor, ct);

			await Task.WhenAll(tasks).AwaitBackground();
		}

		public override void Create(Sensor obj)
		{
			base.Create(obj);
			this.Commit(obj);
		}

		public override async Task<IEnumerable<Sensor>> GetAsync(SensateUser user)
		{
			string key, data;
			IEnumerable<Sensor> sensors;

			key = $"Sensors:uid:{user.Id}";
			data = await this._cache.GetAsync(key);

			if(data != null)
				return JsonConvert.DeserializeObject<IEnumerable<Sensor>>(data);

			sensors = await base.GetAsync(user).AwaitBackground();

			if(sensors == null)
				return null;

			await this._cache.SetAsync( key, JsonConvert.SerializeObject(sensors), CacheTimeout.TimeoutMedium.ToInt() ).AwaitBackground();
			return sensors;
		}

		public override async Task<Sensor> GetAsync(string id)
		{
			string data;
			Sensor sensor;

			data = await this._cache.GetAsync(id).AwaitBackground();

			if(data != null)
				return JsonConvert.DeserializeObject<Sensor>(data);

			sensor = await base.GetAsync(id).AwaitBackground();

			if(sensor == null)
				return null;

			await this.CommitAsync(sensor).AwaitBackground();
			return sensor;

		}

		public override async Task UpdateAsync(Sensor sensor)
		{
			var tasks = new[] {
                this._cache.SetAsync(sensor.InternalId.ToString(), sensor.ToJson()),
                base.UpdateAsync(sensor)
			};

			await Task.WhenAll(tasks).AwaitBackground();
		}

		public override async Task RemoveAsync(string id)
		{
			var tsk = new[] {
				this._cache.RemoveAsync(id),
				base.RemoveAsync(id)
			};

			await Task.WhenAll(tsk).AwaitBackground();
		}
	}
}
