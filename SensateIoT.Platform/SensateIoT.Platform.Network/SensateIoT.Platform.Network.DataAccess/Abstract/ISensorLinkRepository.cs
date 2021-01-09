/*
 * Sensor link repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
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
		Task<bool> DeleteAsync(SensorLink link, CancellationToken token = default);
		Task<IEnumerable<SensorLink>> GetAsync(string sensorId, CancellationToken ct = default);
		Task<IEnumerable<SensorLink>> GetByUserAsync(Guid userId, CancellationToken token = default);
		Task<bool> DeleteBySensorAsync(ObjectId sensor, CancellationToken ct = default);
	}
}
