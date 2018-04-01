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
		Task CreateAsync(T obj);
		void Update(T obj);
		void Delete(TKey id);
		Task DeleteAsync(TKey id);
		void Commit(T obj);
		Task CommitAsync(T obj);
	}
}
