/*
 * Distributed caching strategy
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
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

		public override async Task<string> GetAsync(string key, CancellationToken ct = default(CancellationToken))
		{
			return await this._cache.GetStringAsync(key, ct).AwaitBackground();
		}

		public override void Remove(string key)
		{
			this._cache.Remove(key);
		}

		public override async Task SerializeAsync(string key, object obj, int tmo, bool slide, CancellationToken ct = default(CancellationToken))
		{
			string data;
			DistributedCacheEntryOptions opts;

			data = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
			opts = new DistributedCacheEntryOptions();

			if(slide)
				opts.SetSlidingExpiration(TimeSpan.FromMinutes(tmo));
			else
				opts.SetAbsoluteExpiration(TimeSpan.FromMinutes(tmo));
			await this._cache.SetStringAsync(key, data, opts, ct).AwaitBackground();
		}

		public override async Task<ObjType> DeserializeAsync<ObjType>(string key, CancellationToken ct = default(CancellationToken))
		{
			var data = await this._cache.GetStringAsync(key, ct).AwaitBackground();
			return data == null ? null : Newtonsoft.Json.JsonConvert.DeserializeObject<ObjType>(data);
		}

		public override void Serialize(string key, object obj, int tmo, bool slide)
		{
			byte[] data;
			DistributedCacheEntryOptions opts;

			data = obj.ToByteArray();
			opts = new DistributedCacheEntryOptions();

			if(slide)
				opts.SetSlidingExpiration(TimeSpan.FromMinutes(tmo));
			else
				opts.SetAbsoluteExpiration(TimeSpan.FromMinutes(tmo));
			this._cache.SetAsync(key, data, opts);
		}

		public override ObjType Deserialize<ObjType>(string key)
		{
			byte[] data;

			data = this._cache.Get(key);
			return data.FromByteArray<ObjType>();
		}

		public override async Task RemoveAsync(string key)
		{
			await this._cache.RemoveAsync(key).AwaitBackground();
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
			var options = new DistributedCacheEntryOptions();

			if(slide)
				options.SetSlidingExpiration(TimeSpan.FromMinutes(tmo));
			else
				options.SetAbsoluteExpiration(TimeSpan.FromMinutes(tmo));

			await this._cache.SetStringAsync(key, obj, options, ct).AwaitBackground();
		}
	}
}
