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
		Task CreateAsync(SensorLink link, CancellationToken token = default);
		Task DeleteAsync(SensorLink link, CancellationToken token = default);
		Task DeleteForAsync(Sensor sensor, CancellationToken token = default);
		Task<IEnumerable<SensorLink>> GetAsync(string sensorId, CancellationToken ct = default);
		Task<IEnumerable<SensorLink>> GetByUserAsync(SensateUser user, CancellationToken token = default);
		Task<int> CountAsync(SensateUser user, CancellationToken ct = default);
		Task DeleteBySensorAsync(Sensor sensor, CancellationToken ct = default);
		Task DeleteByUserAsync(SensateUser user, CancellationToken ct = default);
	}
}