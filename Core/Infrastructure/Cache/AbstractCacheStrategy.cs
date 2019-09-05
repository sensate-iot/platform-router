/*
 * Abstract caching strategy
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System.Threading.Tasks;
using System.Threading;

namespace SensateService.Infrastructure.Cache
{
	public abstract class AbstractCacheStrategy : ICacheStrategy<string>
	{
		public abstract string Get(string key);
		public abstract Task<string> GetAsync(string key, CancellationToken ct = default(CancellationToken));
		public abstract void Remove(string key);

		public abstract Task SerializeAsync(string key, object obj, int tmo, bool slide,
			CancellationToken ct = default(CancellationToken));

		public abstract Task<ObjType> DeserializeAsync<ObjType>(string key,
			CancellationToken ct = default(CancellationToken)) where ObjType : class;

		public abstract void Serialize(string key, object obj, int tmo, bool slide);
		public abstract ObjType Deserialize<ObjType>(string key) where ObjType : class;

		public abstract Task RemoveAsync(string key);
		public abstract void Set(string key, string obj);
		public abstract void Set(string key, string obj, int tmo, bool slide = true);
		public abstract Task SetAsync(string key, string obj);
		public abstract Task SetAsync(string key, string obj, int tmo, bool slide = true, CancellationToken ct = default(CancellationToken));
	}
}
