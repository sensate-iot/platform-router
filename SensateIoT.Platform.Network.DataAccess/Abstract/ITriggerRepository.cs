/*
 * Trigger service repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;

using TriggerAction = SensateIoT.Platform.Network.Data.DTO.TriggerAction;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface ITriggerRepository
	{
		Task<IEnumerable<TriggerAction>> GetTriggerServiceActions(CancellationToken ct = default);
		Task<IEnumerable<TriggerAction>> GetTriggerServiceActionsBySensorId(ObjectId id, CancellationToken ct = default);
	}
}
