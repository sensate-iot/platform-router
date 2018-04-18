/*
 * Cached sensor repository.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using SensateService.Infrastructure.Cache;
using SensateService.Models;
using SensateService.Infrastructure.Repositories;

namespace SensateService.Infrastructure.Document
{
	public class CachedSensorRepository : SensorRepository
	{
		private ICacheStrategy<string> _cache;

		public const int CacheTimeout = 10;
		public const int CacheTimeoutShort = 1;

		public CachedSensorRepository(
			SensateContext context,
			IMeasurementRepository measurements,
			ISensorStatisticsRepository stats,
			ILogger<SensorRepository> logger,
			ICacheStrategy<string> cache) : base(context, stats, measurements, logger)
		{
			this._cache = cache;
		}

		public override void Commit(Sensor obj)
		{
			this._cache.Set(obj.InternalId.ToString(), obj.ToJson());
		}

		private async Task CommitAsync(
			Sensor obj,
			CancellationToken ct = default(CancellationToken)
		)
		{
			await this._cache.SetAsync(
				obj.InternalId.ToString(),
				obj.ToJson(),
				CacheTimeout.Timeout.ToInt(),
				true,
				ct
			).AwaitSafely();
		}

		public override async Task CreateAsync(Sensor sensor)
		{
			var tasks = new Task[2];

			tasks[0] = base.CreateAsync(sensor);
			tasks[1] = this.CommitAsync(sensor);

			await Task.WhenAll(tasks).AwaitSafely();
		}

		public override void Create(Sensor obj)
		{
			base.Create(obj);
			this.Commit(obj);
		}

		public override Sensor Get(string id)
		{
			string data;
			Sensor sensor;

			data = this._cache.Get(id);
			if(data != null)
				return JsonConvert.DeserializeObject<Sensor>(data);

			sensor = base.Get(id);
			if(sensor == null)
				return null;

			this.Commit(sensor);
			return sensor;

		}

		public override async Task<Sensor> GetAsync(string id)
		{
			string data;
			Sensor sensor;

			data = await this._cache.GetAsync(id).AwaitSafely();

			if(data != null)
				return JsonConvert.DeserializeObject<Sensor>(data);

			sensor = await base.GetAsync(id).AwaitSafely();

			if(sensor == null)
				return null;

			await this.CommitAsync(sensor).AwaitSafely();
			return sensor;

		}

		public override async Task RemoveAsync(string id)
		{
			await this.DeleteAsync(id).AwaitSafely();
		}

		public override void Remove(string id)
		{
			this.Delete(id);
		}

		public override void Update(Sensor obj)
		{
			this._cache.Set(obj.InternalId.ToString(), obj.ToJson());
			base.Update(obj);
		}

		public override async Task UpdateAsync(Sensor sensor)
		{
			var tasks = new[] {
                this._cache.SetAsync(sensor.InternalId.ToString(), sensor.ToJson()),
                base.UpdateAsync(sensor)
			};

			await Task.WhenAll(tasks);
		}

		public override void Delete(string id)
		{
			this._cache.Remove(id);
			base.Delete(id);
		}

		public override async Task DeleteAsync(string id)
		{
			var tsk = new[] {
				this._cache.RemoveAsync(id),
				base.DeleteAsync(id)
			};

			await Task.WhenAll(tsk);
		}

		public override Sensor GetById(string id)
		{
			return this.Get(id);
		}
	}
}
