/*
 * Abstract caching strategy
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching;

namespace SensateService.Infrastructure.Cache
{
	public abstract class AbstractCacheStrategy : ICacheStrategy<string>
	{
		public const int CacheTimeout = 10;

		public abstract string Get(string key);
		public abstract Task<string> GetAsync(string key);
		public abstract void Remove(string key);
		public abstract Task RemoveAsync(string key);
		public abstract void Set(string key, string obj);
		public abstract void Set(string key, string obj, int tmo, bool slide = true);
		public abstract Task SetAsync(string key, string obj);
		public abstract Task SetAsync(string key, string obj, int tmo, bool slide = true);
	}
}
