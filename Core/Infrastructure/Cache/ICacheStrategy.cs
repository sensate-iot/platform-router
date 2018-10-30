/*
 * Data caching strategy interface
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SensateService.Infrastructure.Cache
{
	public interface ICacheStrategy<T> where T : class
	{
		T Get(string key);
		void Set(string key, T obj);
		void Set(string key, T obj, int tmo, bool slide = true);

		Task<T> GetAsync(string key, CancellationToken ct = default(CancellationToken));
		Task SetAsync(string key, T obj);
		Task SetAsync(string key, T obj, int tmo, bool slide = true, CancellationToken ct = default(CancellationToken));

		Task RemoveAsync(string key);
		void Remove(string key);

		Task SerializeAsync(string key, object obj, int tmo, bool slide, CancellationToken ct = default(CancellationToken));

		Task<ObjType> DeserializeAsync<ObjType>(string key, CancellationToken ct = default(CancellationToken))
			where ObjType : class;

		void Serialize(string key, object obj, int tmo, bool slide);
		ObjType Deserialize<ObjType>(string key) where ObjType : class;
	}
}
