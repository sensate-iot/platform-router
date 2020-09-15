/*
 * Data caching strategy interface
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System.Threading;
using System.Threading.Tasks;

namespace SensateService.Infrastructure.Cache
{
	public interface ICacheStrategy<T> where T : class
	{
		Task<T> GetAsync(string key, CancellationToken ct = default);
		Task SetAsync(string key, T obj);
		Task SetAsync(string key, T obj, int tmo, CancellationToken ct = default);
		Task RemoveAsync(string key, CancellationToken ct = default);
	}
}
