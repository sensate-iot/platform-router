/*
 * Sensor aggregation service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System.Threading.Tasks;
using SensateService.Common.Data.Dto.Json.Out;
using SensateService.Common.Data.Models;
using SensateService.Common.IdentityData.Models;

namespace SensateService.Services
{
	public interface ISensorService
	{
		Task<PaginationResult<Sensor>> GetSensorsAsync(SensateUser user, int skip = 0, int limit = 0, CancellationToken token = default);
		Task<PaginationResult<Sensor>> GetSensorsAsync(SensateUser user, string name, int skip = 0, int limit = 0, CancellationToken token = default);
		Task DeleteAsync(Sensor sensor, CancellationToken ct = default);
	}
}
