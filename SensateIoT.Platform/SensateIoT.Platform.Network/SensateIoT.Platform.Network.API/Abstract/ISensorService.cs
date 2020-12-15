/*
 * Sensor aggregation service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System.Threading.Tasks;
using SensateIoT.Platform.Network.API.DTO;
using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.API.Abstract
{
	public interface ISensorService
	{
		Task<PaginationResult<Sensor>> GetSensorsAsync(User user, int skip = 0, int limit = 0, CancellationToken token = default);
		Task<PaginationResult<Sensor>> GetSensorsAsync(User user, string name, int skip = 0, int limit = 0, CancellationToken token = default);
		Task DeleteAsync(Sensor sensor, CancellationToken ct = default);
	}
}
