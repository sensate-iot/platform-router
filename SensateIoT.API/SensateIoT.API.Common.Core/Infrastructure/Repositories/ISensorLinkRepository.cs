/*
 * Repository for the sensor link model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SensateIoT.API.Common.Data.Models;
using SensateIoT.API.Common.IdentityData.Models;

namespace SensateIoT.API.Common.Core.Infrastructure.Repositories
{
	public interface ISensorLinkRepository
	{
		Task<IEnumerable<SensorLink>> GetByUserAsync(SensateUser user, CancellationToken token = default);
		Task<long> CountAsync(SensateUser user, CancellationToken ct = default);
	}
}