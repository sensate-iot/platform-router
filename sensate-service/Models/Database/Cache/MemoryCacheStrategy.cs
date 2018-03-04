/*
 * Distributed caching strategy
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching;

namespace SensateService.Models.Database.Cache
{
	public sealed class MemoryCacheStrategy : AbstractCacheStrategy
	{
		private IMemoryCache _cache;

		public MemoryCacheStrategy(IMemoryCache cache)
		{
			this._cache = cache;
		}

		public override string Get(string key)
		{
			return this._cache.Get<string>(key);
		}

		public override async Task<string> GetAsync(string key)
		{
			return await Task.Run(() => {
				return this._cache.Get<string>(key);
			});
		}

		public override void Remove(string key)
		{
			this._cache.Remove(key);
		}

		public override async Task RemoveAsync(string key)
		{
			await Task.Run(() => this._cache.Remove(key));
		}

		public override void Set(string key, string obj)
		{
			this.Set(key, obj, CacheTimeout);
		}

		public override void Set(string key, string obj, int tmo)
		{
			this._cache.Set(key, obj, new MemoryCacheEntryOptions()
				.SetSlidingExpiration(TimeSpan.FromMinutes(tmo)));
		}

		public override async Task SetAsync(string key, string obj)
		{
			await this.SetAsync(key, obj, CacheTimeout);
		}

		public override async Task SetAsync(string key, string obj, int tmo)
		{
			await Task.Run(() => this._cache.Set(
				key, obj,
				new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(tmo)))
			);
		}
	}
}
