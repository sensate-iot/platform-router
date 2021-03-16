using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Data.Models;

using TriggerAction = SensateIoT.Platform.Network.Data.DTO.TriggerAction;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface ITriggerRepository
	{
		Task<IEnumerable<TriggerAction>> GetTriggerServiceActions(CancellationToken ct = default);
		Task<IEnumerable<TriggerAction>> GetTriggerServiceActionsBySensorId(ObjectId id, CancellationToken ct = default);
		Task StoreTriggerInvocation(TriggerInvocation invocation, CancellationToken ct = default);
	}
}