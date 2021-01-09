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
		Task<IEnumerable<TriggerAction>> GetTriggerServiceActions(IEnumerable<ObjectId> sensorIds, CancellationToken ct = default);
		Task StoreTriggerInvocation(TriggerInvocation invocation, CancellationToken ct = default);
		Task<IEnumerable<Trigger>> GetAsync(string sensorId, TriggerType type, CancellationToken ct = default);
		Task<Trigger> GetAsync(long id, CancellationToken ct = default);
		Task RemoveActionAsync(Trigger trigger, TriggerChannel channel, CancellationToken ct = default);
		Task AddActionsAsync(Trigger trigger, IEnumerable<Data.Models.TriggerAction> actions, CancellationToken ct = default);
		Task AddActionAsync(Trigger trigger, Data.Models.TriggerAction action, CancellationToken ct = default);
		Task DeleteAsync(long id, CancellationToken ct = default);
		Task DeleteBySensorAsync(string sensorId, CancellationToken ct = default);
		Task CreateAsync(Trigger trigger, CancellationToken ct = default);
	}
}
