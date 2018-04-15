/*
 * Abstract measurement repository
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System.Threading.Tasks;
using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface ISensorRepository
	{
		void Create(Sensor sensor);
		Task CreateAsync(Sensor sensor);
		Sensor Get(string id);
		void Remove(string secret);
		void Update(Sensor obj);

		Task<Sensor> GetAsync(string id);
		Task RemoveAsync(string id);
		Task UpdateAsync(Sensor sensor);
	}
}
