/*
 * Distributed caching strategy
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Threading.Tasks;
using System.Threading;

using SensateService.Common.Caching.Abstract;

namespace SensateService.Infrastructure.Cache
{
	public sealed class DistributedCacheStrategy : AbstractCacheStrategy
	{
		private readonly IDistributedCache<string> _cache;

		public DistributedCacheStrategy(IDistributedCache<string> cache)
		{
			this._cache = cache;
		}

		public override async Task<string> GetAsync(string key, CancellationToken ct = default)
		{
			string rv = null;
			try {
				rv = await this._cache.GetAsync(key, ct).ConfigureAwait(false);
			} catch(ArgumentOutOfRangeException) { }

			return rv;
		}

		public override Task RemoveAsync(string key, CancellationToken ct = default)
		{
			return this._cache.RemoveAsync(key, ct);
		}

		public override Task SetAsync(string key, string obj)
		{
			return this._cache.SetAsync(key, obj);
		}

		public override Task SetAsync(
			string key,
			string obj,
			int tmo,
			CancellationToken ct = default
		)
		{
			var options = new CacheEntryOptions {
				Timeout = TimeSpan.FromMinutes(tmo)
			};

			return this._cache.SetAsync(key, obj, options, ct);
		}
	}
}
