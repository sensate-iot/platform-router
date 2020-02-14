/*
 * Abstract measurement repository
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface ISensorRepository
	{
		Task CreateAsync(Sensor sensor, CancellationToken ct = default(CancellationToken));

		Task<IEnumerable<Sensor>> GetAsync(SensateUser user);
		Task<Sensor> GetAsync(string id);
		Task<IEnumerable<Sensor>> GetAsync(IEnumerable<string> ids);
		Task<IEnumerable<Sensor>> FindByNameAsync(SensateUser user, string name);

		Task<long> CountAsync(SensateUser user = null);
		Task RemoveAsync(Sensor sensor);
		Task UpdateAsync(Sensor sensor);
		Task UpdateSecretAsync(Sensor sensor, SensateApiKey key);
	}
}
