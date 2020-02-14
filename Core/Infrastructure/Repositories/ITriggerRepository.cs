/*
 * Trigger data access repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface ITriggerRepository
	{
		public Task CreateAsync(Trigger trigger, CancellationToken ct = default);
		public Task UpdateAsync(Trigger trigger, CancellationToken ct = default);
		public Task<Trigger> GetAsync(long id, CancellationToken ct = default);
		public Task<IEnumerable<Trigger>> GetAsync(string id, CancellationToken ct = default);
		public Task<IEnumerable<Trigger>> GetAsync(IEnumerable<string> ids, CancellationToken ct = default);
		public Task DeleteAsync(long id, CancellationToken ct = default);
		public Task DeleteBySensorAsync(string sensorId, CancellationToken ct = default);
		public Task AddActionAsync(Trigger trigger, TriggerAction action, CancellationToken ct = default);
		public Task AddActionsAsync(Trigger trigger, IEnumerable<TriggerAction> action, CancellationToken ct = default);
		public Task RemoveActionAsync(Trigger trigger, TriggerActionChannel id, CancellationToken ct = default);
		public Task AddInvocationAsync(Trigger trigger, TriggerInvocation invocation, CancellationToken ct = default);
		public Task AddInvocationsAsync(IEnumerable<TriggerInvocation> invocations, CancellationToken ct = default);
	}
}

