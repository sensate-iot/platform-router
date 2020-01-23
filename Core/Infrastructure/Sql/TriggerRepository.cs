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
			await this.EndUpdateAsync(ct).AwaitBackground();
		}

		public async Task<Trigger> GetAsync(long id, CancellationToken ct = default)
		{
			var query = this.Data.Where(t => t.Id == id)
				.Include(a => a.Actions);
			var trigger = await query.FirstOrDefaultAsync(ct).AwaitBackground();

			trigger.LastTriggered = new DateTime(trigger.LastTriggered.Ticks, DateTimeKind.Utc);
			return trigger;
		}

		public async Task<IEnumerable<Trigger>> GetAsync(string id, CancellationToken ct = default)
		{
			var query = this.Data.Where(trigger => trigger.SensorId == id)
				.Include(a => a.Actions);
			var triggers = await query.ToListAsync(ct).AwaitBackground();

			foreach(var trigger in triggers) {
				trigger.LastTriggered = new DateTime(trigger.LastTriggered.Ticks, DateTimeKind.Utc);
			}

			return triggers;
		}

		public async Task<IEnumerable<Trigger>> GetAsync(IEnumerable<string> ids, CancellationToken ct = default)
		{
			var query = this.Data.Where(t => ids.Contains(t.SensorId))
				.Include(a => a.Actions);
			var triggers = await query.ToListAsync(ct).AwaitBackground();

			foreach(var trigger in triggers) {
				trigger.LastTriggered = new DateTime(trigger.LastTriggered.Ticks, DateTimeKind.Utc);
			}

			return triggers;
		}

		public async Task DeleteAsync(long id, CancellationToken ct = default)
		{
			var entity = await this.Data.Where(t => t.Id == id).FirstOrDefaultAsync(ct).AwaitBackground();

			if(entity == null)
				return;

			this.Data.Remove(entity);
			await this.CommitAsync(ct).AwaitBackground();
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

		public async Task UpdateTriggerTimestampAsync(Trigger trigger, CancellationToken ct = default)
		{
			this.StartUpdate(trigger);
			trigger.LastTriggered = DateTime.Now.ToUniversalTime();
			await this.CommitAsync(ct).AwaitBackground();
		}
	}
}

