/*
 * Data caching strategy interface
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Threading.Tasks;

namespace SensateService.Models.Database.Cache
{
	public interface ICacheStrategy<T> where T : class
	{
		T Get(string key);
		void Set(string key, T obj);
		void Set(string key, T obj, int tmo);

		Task<T> GetAsync(string key);
		Task SetAsync(string key, T obj);
		Task SetAsync(string key, T obj, int tmo);

		Task RemoveAsync(string key);
		void Remove(string key);
	}
}
