/*
 * Distributed caching strategy
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Caching.Memory;
using SensateService.Common.Data.Enums;
using SensateService.Helpers;

namespace SensateService.Infrastructure.Cache
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

		public override async Task<string> GetAsync(string key, CancellationToken ct = default(CancellationToken))
		{
			if(string.IsNullOrEmpty(key))
				return null;

			return await Task.Run(() => this._cache.Get<string>(key), ct).AwaitBackground();
		}

		public override void Remove(string key)
		{
			this._cache.Remove(key);
		}

		public override async Task SerializeAsync(string key, object obj, int tmo, bool slide, CancellationToken ct = default(CancellationToken))
		{
			var options = new MemoryCacheEntryOptions();

			if(slide)
				options.SetSlidingExpiration(TimeSpan.FromMinutes(tmo));
			else
				options.SetAbsoluteExpiration(TimeSpan.FromMinutes(tmo));

			this._cache.Set(key, obj, options);
			await Task.CompletedTask;
		}

		public override async Task<ObjType> DeserializeAsync<ObjType>(string key, CancellationToken ct = default(CancellationToken))
		{
			if(!this._cache.TryGetValue(key, out var obj))
				return null;

			await Task.CompletedTask;
			return obj as ObjType;
		}

		public override void Serialize(string key, object obj, int tmo, bool slide)
		{
			var options = new MemoryCacheEntryOptions();

			if(slide)
				options.SetSlidingExpiration(TimeSpan.FromMinutes(tmo));
			else
				options.SetAbsoluteExpiration(TimeSpan.FromMinutes(tmo));

			this._cache.Set(key, obj, options);
		}

		public override ObjType Deserialize<ObjType>(string key)
		{
			if(!this._cache.TryGetValue(key, out object obj))
				return null;

			return obj as ObjType;
		}

		public override async Task RemoveAsync(string key)
		{
			await Task.Run(() => this._cache.Remove(key)).AwaitBackground();
		}

		public override void Set(string key, string obj)
		{
			this.Set(key, obj, CacheTimeout.Timeout.ToInt());
		}

		public override void Set(string key, string obj, int tmo, bool slide = true)
		{
			this._cache.Set(key, obj, new MemoryCacheEntryOptions()
				.SetSlidingExpiration(TimeSpan.FromMinutes(tmo)));
		}

		public override async Task SetAsync(string key, string obj)
		{
			await this.SetAsync(key, obj, CacheTimeout.Timeout.ToInt()).AwaitBackground();
		}

		public override async Task SetAsync(
			string key,
			string obj,
			int tmo,
			bool slide = true,
			CancellationToken ct = default(CancellationToken)
		)
		{
			await Task.Run(() => this._cache.Set(
				key, obj,
				new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(tmo))),
				ct
			).AwaitBackground();
		}
	}
}
