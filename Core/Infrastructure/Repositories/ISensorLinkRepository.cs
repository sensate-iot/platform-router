/*
 * Repository for the sensor link model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface ISensorLinkRepository
	{
		Task CreateAsync(SensorLink link, CancellationToken token = default);
		Task DeleteAsync(SensorLink link, CancellationToken token = default);
		Task<IEnumerable<SensorLink>> GetByUserAsync(SensateUser user, CancellationToken token = default);
	}
}