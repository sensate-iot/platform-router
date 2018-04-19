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
using SensateService.Helpers;

namespace SensateService.Infrastructure.Cache
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

		public override async Task<string> GetAsync(string key, default(CancellationToken))
		{
			return await this._cache.GetStringAsync(key, ct).AwaitSafely();
		}

		public override void Remove(string key)
		{
			this._cache.Remove(key);
		}

		public override async Task RemoveAsync(string key)
		{
			await this._cache.RemoveAsync(key).AwaitSafely();
		}

		public override void Set(string key, string obj)
		{
			this.Set(key, obj, CacheTimeout.Timeout.ToInt());
		}

		public override void Set(string key, string obj, int tmo, bool slide = true)
		{
			DistributedCacheEntryOptions options;

			options = new DistributedCacheEntryOptions();

			if(slide)
				options.SetSlidingExpiration(TimeSpan.FromMinutes(tmo));
			else
				options.SetAbsoluteExpiration(TimeSpan.FromMinutes(tmo));

			this._cache.SetString(key, obj, options);
		}

		public override async Task SetAsync(string key, string obj)
		{
			await this.SetAsync(key, obj, CacheTimeout.Timeout.ToInt()).AwaitSafely();
		}

		public override async Task SetAsync(
			string key,
			string obj,
			int tmo,
			bool slide = true,
			CancellationToken ct = default(CancellationToken)
		)
		{
			var options = new DistributedCacheEntryOptions();

			if(slide)
				options.SetSlidingExpiration(TimeSpan.FromMinutes(tmo));
			else
				options.SetAbsoluteExpiration(TimeSpan.FromMinutes(tmo));

			await this._cache.SetStringAsync(key, obj, options, ct).AwaitSafely();
		}
	}
}
