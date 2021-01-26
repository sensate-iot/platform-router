/*
 * Abstract measurement repository
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using SensateIoT.API.Common.Data.Models;
using SensateIoT.API.Common.IdentityData.Models;

namespace SensateIoT.API.Common.Core.Infrastructure.Repositories
{
	public interface ISensorRepository
	{
		Task<IEnumerable<Sensor>> GetAsync(SensateUser user, int skip = 0, int limit = 0);
		Task<Sensor> GetAsync(string id);
		Task<IEnumerable<Sensor>> GetAsync(IEnumerable<string> ids);

		Task<long> CountAsync(SensateUser user = null);
	}
}
