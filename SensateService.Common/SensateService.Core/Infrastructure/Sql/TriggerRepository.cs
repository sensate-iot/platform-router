/*
 * Trigger data layer implementation.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class TriggerRepository : AbstractSqlRepository<Trigger>, ITriggerRepository
	{
		public TriggerRepository(SensateSqlContext context) : base(context)
		{
		}

		public async Task UpdateAsync(Trigger trigger, CancellationToken ct = default)
		{
			var value = await this.Data.FirstOrDefaultAsync(t => t.Id == trigger.Id, ct).AwaitBackground();

			this.StartUpdate(value);
			value.SensorId = trigger.SensorId;
			value.UpperEdge = trigger.UpperEdge;
			value.LowerEdge = trigger.LowerEdge;
			value.KeyValue = trigger.KeyValue;
			value.FormalLanguage = trigger.FormalLanguage;
			value.Type = trigger.Type;
			await this.EndUpdateAsync(ct).AwaitBackground();
		}

		public async Task<Trigger> GetAsync(long id, CancellationToken ct = default)
		{
			var query = this.Data.Where(t => t.Id == id)
				.Include(t => t.Actions)
				.Include(t => t.Invocations);
			var trigger = await query.FirstOrDefaultAsync(ct).AwaitBackground();

			return trigger;
		}

		public async Task<IEnumerable<Trigger>> GetAsync(string id, TriggerType type = TriggerType.Number, CancellationToken ct = default)
		{
			var query = this.Data.Where(trigger => trigger.SensorId == id && trigger.Type == type)
				.Include(t => t.Actions)
				.Include(t => t.Invocations);
			var triggers = await query.ToListAsync(ct).AwaitBackground();

			return triggers;
		}

		public async Task<IEnumerable<Trigger>> GetByTypeAsync(TriggerType type, CancellationToken ct = default)
		{
			var query = this.Data.Where(trigger => trigger.Type == type)
				.Include(t => t.Actions)
				.Include(t => t.Invocations);
			var triggers = await query.ToListAsync(ct).AwaitBackground();

			return triggers;
		}

		public async Task<IEnumerable<Trigger>> GetAsync(IEnumerable<string> ids,
														 TriggerType type = TriggerType.Number,
														 CancellationToken ct = default)
		{
			var query = this.Data.Where(
					t => ids.Contains(t.SensorId) &&
					t.Type == type
				)
				.Include(t => t.Invocations)
				.Include(t => t.Actions);

			var triggers = await query.ToListAsync(ct).AwaitBackground();

			return triggers;
		}

		public async Task DeleteAsync(long id, CancellationToken ct = default)
		{
			var entity = await this.Data.Where(t => t.Id == id).FirstOrDefaultAsync(ct).AwaitBackground();

			if(entity == null) {
				return;
			}

			this.Data.Remove(entity);
			await this.CommitAsync(ct).AwaitBackground();
		}

		public Task DeleteBySensorAsync(string sensorId, CancellationToken ct = default)
		{
			var where = this.Data.Where(t => t.SensorId == sensorId);

			if(!@where.Any()) {
				return Task.CompletedTask;
			}
			this.Data.RemoveRange(where);

			return this.CommitAsync(ct);
		}

		public async Task AddActionAsync(Trigger trigger, TriggerAction action, CancellationToken ct = default)
		{
			this._sqlContext.TriggerActions.Add(action);

			await this.CommitAsync(ct).AwaitBackground();
			trigger.Actions.Add(action);
		}

		public async Task AddActionsAsync(Trigger trigger, IEnumerable<TriggerAction> actions, CancellationToken ct = default)
		{
			var list = actions.ToList();

			this._sqlContext.TriggerActions.AddRange(list);
			await this.CommitAsync(ct).AwaitBackground();

			foreach(var action in list) {
				trigger.Actions.Add(action);
			}
		}

		public async Task RemoveActionAsync(Trigger trigger, TriggerActionChannel id, CancellationToken ct = default)
		{
			var action = trigger.Actions.FirstOrDefault(x => x.TriggerId == trigger.Id && x.Channel == id);

			if(action == null)
				return;

			this._sqlContext.TriggerActions.Remove(action);
			await this.CommitAsync(ct).AwaitBackground();

			trigger.Actions.Remove(action);
		}

		public async Task AddInvocationAsync(Trigger trigger, TriggerInvocation invocation, CancellationToken ct = default)
		{
			invocation.Timestamp = DateTimeOffset.UtcNow;

			this._sqlContext.TriggerInvocations.Add(invocation);
			await this.CommitAsync(ct).AwaitBackground();
		}

		public Task AddInvocationsAsync(IEnumerable<TriggerInvocation> invocations, CancellationToken ct = default)
		{
			this._sqlContext.TriggerInvocations.AddRange(invocations);
			return this.CommitAsync(ct);
		}
	}
}

