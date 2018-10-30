/*
 * Repository interface.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System.Threading.Tasks;

namespace SensateService.Infrastructure
{
	public interface IRepository<in TKey, T> where T : class
	{
		T GetById(TKey id);
		void Create(T obj);
		Task CreateAsync(T obj);
		void Update(T obj);
		void Delete(TKey id);
		Task DeleteAsync(TKey id);
	}
}
