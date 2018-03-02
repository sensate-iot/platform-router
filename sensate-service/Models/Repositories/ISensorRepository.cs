/*
 * Abstract measurement repository
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SensateService.Models.Repositories
{
	public interface ISensorRepository
	{
		bool Create(Sensor sensor);
		Sensor Get(string id);
		void Remove(string secret);
		bool Update(Sensor obj2);

		Task<Boolean> CreateAsync(Sensor sensor);
		Task<Sensor> GetAsync(string id);
		Task RemoveAsync(string id);
		Task<Boolean> UpdateAsync(Sensor sensor);
	}
}
