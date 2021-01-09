/*
 * Repository for the sensor link model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SensateService.Common.Data.Models;
using SensateService.Common.IdentityData.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface ISensorLinkRepository
	{
		Task<IEnumerable<SensorLink>> GetByUserAsync(SensateUser user, CancellationToken token = default);
		Task<long> CountAsync(SensateUser user, CancellationToken ct = default);
	}
}