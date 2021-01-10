/*
 * Distributed caching strategy
 *
 * @author: Michel Megens
 * @email:  michel@michelmegens.net
 */

using System;
using System.Threading.Tasks;
using System.Threading;

using SensateService.Common.Data.Enums;
using SensateService.Helpers;
using SensateIoT.Common.Caching.Abstract;

namespace SensateService.Infrastructure.Cache
{
	public sealed class MemoryCacheStrategy : AbstractCacheStrategy
	{
		private readonly IMemoryCache<string, string> m_cache;

		public MemoryCacheStrategy(IMemoryCache<string, string> cache)
		{
			this.m_cache = cache;
		}

		public override Task<string> GetAsync(string key, CancellationToken ct = default)
		{
			if(string.IsNullOrEmpty(key)) {
				return null;
			}

			this.m_cache.TryGetValue(key, out var result);
			return Task.FromResult(result);
		}

		public override Task RemoveAsync(string key, CancellationToken ct = default)
		{
			this.m_cache.TryRemove(key);
			return Task.CompletedTask;
		}

		public override async Task SetAsync(string key, string obj)
		{
			await this.SetAsync(key, obj, CacheTimeout.Timeout.ToInt()).AwaitBackground();
		}

		public override Task SetAsync(
			string key,
			string obj,
			int tmo,
			CancellationToken ct = default
		)
		{
			var options = new CacheEntryOptions { Timeout = TimeSpan.FromMinutes(tmo) };

			this.m_cache.AddOrUpdate(key, obj, options);
			return Task.CompletedTask;
		}
	}
}
