/*
 * Cached sensor repository.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson;

using SensateService.Infrastructure.Repositories;
using SensateService.Infrastructure.Cache;
using SensateService.Models;

namespace SensateService.Infrastructure.Document
{
	public class CachedSensorRepository : StandardSensorRepository
	{
		private ICacheStrategy<string> _cache;

		public const int CacheTimeout = 10;
		public const int CacheTimeoutShort = 1;

		public CachedSensorRepository(
			SensateContext context,
			ILogger<StandardSensorRepository> logger,
			ICacheStrategy<string> cache) : base(context, logger)
		{
			this._cache = cache;
		}

		public override void Commit(Sensor obj)
		{
			this._cache.Set(obj.InternalId.ToString(), obj.ToJson());
		}

		public async override Task CommitAsync(Sensor obj)
		{
			await this._cache.SetAsync(obj.InternalId.ToString(), obj.ToJson());
		}

		public override Sensor Get(string id)
		{
			string data;
			Sensor sensor;

			data = this._cache.Get(id);
			if(data == null) {
				sensor = base.Get(id);
				if(sensor == null)
					return null;

				this.Commit(sensor);
				return sensor;
			}

			return JsonConvert.DeserializeObject<Sensor>(data);
		}

		public override async Task<Sensor> GetAsync(string id)
		{
			string data;
			Sensor sensor;

			data = await this._cache.GetAsync(id);
			if(data == null) {
				sensor = await base.GetAsync(id);
				if(sensor == null)
					return null;

				await this.CommitAsync(sensor);
				return sensor;
			}

			return JsonConvert.DeserializeObject<Sensor>(data);
		}

		public override async Task RemoveAsync(string id)
		{
			await this._cache.RemoveAsync(id);
			await base.RemoveAsync(id);
		}

		public override bool Replace(Sensor obj1, Sensor obj2)
		{
			this._cache.Set(obj1.InternalId.ToString(), obj2.ToJson());
			return base.Replace(obj1, obj2);
		}

		public override bool Update(Sensor obj)
		{
			this._cache.Set(obj.InternalId.ToString(), obj.ToJson());
			return base.Update(obj);
		}

		public override async Task<Boolean> UpdateAsync(Sensor sensor)
		{
			await this._cache.SetAsync(sensor.InternalId.ToString(), sensor.ToJson());
			return await base.UpdateAsync(sensor);
		}

		public override bool Delete(string id)
		{
			this._cache.Remove(id);
			return base.Delete(id);
		}

		public override Sensor GetById(string id)
		{
			return this.Get(id);
		}
	}
}