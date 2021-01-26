/*
 * Distributed redis cache.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SensateIoT.Common.Caching.Abstract
{
	public interface IDistributedCache<TValue> : IDisposable where TValue : class
	{
		Task SetAsync(string key, TValue value, CacheEntryOptions options = null, CancellationToken ct = default);
		Task<TValue> GetAsync(string key, CancellationToken ct = default);
		Task RemoveAsync(string key, CancellationToken ct = default);
		Task SetRangeAsync(ICollection<KeyValuePair<string, TValue>> values, CacheEntryOptions options = null, CancellationToken ct = default);
		Task<IEnumerable<TValue>> GetRangeAsync(ICollection<string> keys, CancellationToken ct = default);
	}
}
