/*
 * Sensor trigger information.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public interface ITriggerRepository
	{
		Task<TriggerRoutingInfo> GetTriggerInfoAsync(ObjectId sensorID, CancellationToken ct);
		Task<IEnumerable<TriggerRoutingInfo>> GetTriggerInfoAsync(CancellationToken ct);
	}
}
