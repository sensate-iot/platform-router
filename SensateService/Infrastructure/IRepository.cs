/*
 * Repository interface.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Threading.Tasks;

namespace SensateService.Infrastructure
{
	public interface IRepository<TKey, T> where T : class
	{
		T GetById(TKey id);
		void Create(T obj);
		bool Replace(T obj1, T obj2);
		bool Update(T obj);
		bool Delete(TKey id);
		void Commit(T obj);
		Task CommitAsync(T obj);
	}
}
