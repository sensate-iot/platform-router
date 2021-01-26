/*
 * Abstract caching strategy
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System.Threading;
using System.Threading.Tasks;

namespace SensateIoT.API.Common.Core.Infrastructure.Cache
{
	public abstract class AbstractCacheStrategy : ICacheStrategy<string>
	{
		public abstract Task<string> GetAsync(string key, CancellationToken ct = default);
		public abstract Task RemoveAsync(string key, CancellationToken ct = default);
		public abstract Task SetAsync(string key, string obj);
		public abstract Task SetAsync(string key, string obj, int tmo, CancellationToken ct = default);
	}
}
