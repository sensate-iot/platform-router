/*
 * Sensor link repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface ISensorLinkRepository
	{
		Task CreateAsync(SensorLink link, CancellationToken token = default);
		Task DeleteAsync(SensorLink link, CancellationToken token = default);
		Task<IEnumerable<SensorLink>> GetAsync(string sensorId, CancellationToken ct = default);
		Task<IEnumerable<SensorLink>> GetByUserAsync(string userId, CancellationToken token = default);
		Task<int> CountAsync(string userId, CancellationToken ct = default);
		Task DeleteBySensorAsync(ObjectId sensor, CancellationToken ct = default);
	}
}
