/*
 * Sensor aggregation service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SensateService.Models;

namespace SensateService.Services
{
	public interface ISensorService
	{
		Task<IEnumerable<Sensor>> GetSensorsAsync(SensateUser user, CancellationToken token = default);
		Task<IEnumerable<Sensor>> GetSensorsAsync(SensateUser user, string name, CancellationToken token = default);
	}
}
