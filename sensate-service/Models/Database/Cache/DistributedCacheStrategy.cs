/*
 * Distributed caching strategy
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching;

namespace SensateService.Models.Database.Cache
{
	public sealed class DistributedCacheStrategy : AbstractCacheStrategy
	{
		private IDistributedCache _cache;

		public DistributedCacheStrategy(IDistributedCache cache)
		{
			this._cache = cache;
		}

		public override string Get(string key)
		{
			return this._cache.GetString(key);
		}

		public override async Task<string> GetAsync(string key)
		{
			return await this._cache.GetStringAsync(key);
		}

		public override void Remove(string key)
		{
			this._cache.Remove(key);
		}

		public async override Task RemoveAsync(string key)
		{
			await this._cache.RemoveAsync(key);
		}

		public override void Set(string key, string obj)
		{
			this.Set(key, obj, CacheTimeout);
		}

		public override void Set(string key, string obj, int tmo)
		{
			this._cache.SetString(key, obj, new DistributedCacheEntryOptions()
				.SetSlidingExpiration(TimeSpan.FromMinutes(tmo)));
		}

		public async override Task SetAsync(string key, string obj)
		{
			await this.SetAsync(key, obj, CacheTimeout);
		}

		public async override Task SetAsync(string key, string obj, int tmo)
		{
			await this._cache.SetStringAsync(key, obj,
				new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(tmo)));
		}
	}
}
