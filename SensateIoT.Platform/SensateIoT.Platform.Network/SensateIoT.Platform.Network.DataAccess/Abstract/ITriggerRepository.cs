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

using TriggerAction = SensateIoT.Platform.Network.Data.DTO.TriggerAction;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface ITriggerRepository
	{
		Task<TriggerRoutingInfo> GetTriggerInfoAsync(ObjectId sensorID, CancellationToken ct);
		Task<IEnumerable<TriggerRoutingInfo>> GetTriggerInfoAsync(CancellationToken ct);

		Task<IEnumerable<TriggerAction>> GetTriggerServiceActions(IEnumerable<ObjectId> sensorIds, CancellationToken ct = default);
		Task StoreTriggerInvocation(TriggerInvocation invocation, CancellationToken ct);
	}
}
