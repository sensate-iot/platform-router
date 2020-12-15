/*
 * Sensor trigger information.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MongoDB.Bson;
using Npgsql;

using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Contexts;

using TriggerAction = SensateIoT.Platform.Network.Data.Models.TriggerAction;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class TriggerRepository : ITriggerRepository
	{
		private readonly NetworkContext m_ctx;

		private const string TriggerService_GetTriggersBySensorID = "triggerservice_gettriggersbysensorid";

		public TriggerRepository(NetworkContext ctx)
		{
			this.m_ctx = ctx;
		}

		public async Task<IEnumerable<Data.DTO.TriggerAction>> GetTriggerServiceActions(IEnumerable<ObjectId> sensorIds, CancellationToken ct)
		{
			var result = new List<Data.DTO.TriggerAction>();

			await using var cmd = this.m_ctx.Database.GetDbConnection().CreateCommand();
			if(cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			var idArray = string.Join(",", sensorIds);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = TriggerService_GetTriggersBySensorID;
			cmd.Parameters.Add(new NpgsqlParameter("idlist", DbType.String) { Value = idArray });

			await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
			while(await reader.ReadAsync(ct)) {
				var record = new Data.DTO.TriggerAction {
					TriggerID = reader.GetInt64(0),
					ActionID = reader.GetInt64(1),
					SensorID = ObjectId.Parse(reader.GetString(2)),
					KeyValue = reader.GetString(3),
					FormalLanguage = !await reader.IsDBNullAsync(6, ct) ? reader.GetString(6) : null,
					Type = (TriggerType)reader.GetFieldValue<int>(7),
					Channel = (TriggerChannel)reader.GetFieldValue<int>(8),
					Target = !await reader.IsDBNullAsync(9, ct) ? reader.GetString(9) : null,
					Message = !await reader.IsDBNullAsync(10, ct) ? reader.GetString(10) : null,
					LastInvocation = !await reader.IsDBNullAsync(11, ct) ? reader.GetDateTime(11) : DateTime.MinValue
				};

				if(await reader.IsDBNullAsync(4, ct)) {
					record.LowerEdge = null;
				} else {
					record.LowerEdge = reader.GetDecimal(4);
				}

				if(await reader.IsDBNullAsync(5, ct)) {
					record.UpperEdge = null;
				} else {
					record.UpperEdge = reader.GetDecimal(5);
				}

				result.Add(record);
			}

			return result;
		}

		public async Task StoreTriggerInvocation(TriggerInvocation invocation, CancellationToken ct)
		{
			await this.m_ctx.TriggerInvocations.AddAsync(invocation, ct);
			await this.m_ctx.SaveChangesAsync(ct).ConfigureAwait(false);
		}

		public async Task<IEnumerable<Trigger>> GetAsync(string sensorId, CancellationToken ct = default)
		{
			var query = this.m_ctx.Triggers.Where(x => x.SensorID == sensorId)
				.Include(t => t.TriggerActions);

			return await query.ToListAsync(ct).ConfigureAwait(false);
		}

		public async Task RemoveActionAsync(Trigger trigger, TriggerChannel id, CancellationToken ct = default)
		{
			var action = trigger.TriggerActions.FirstOrDefault(x => x.TriggerID == trigger.ID && x.Channel == id);

			if(action == null)
				return;

			this.m_ctx.TriggerActions.Remove(action);
			await this.m_ctx.SaveChangesAsync(ct).ConfigureAwait(false);

			trigger.TriggerActions.Remove(action);
		}

		public async Task AddActionAsync(Trigger trigger, TriggerAction action, CancellationToken ct = default)
		{
			await this.m_ctx.TriggerActions.AddAsync(action, ct).ConfigureAwait(false);
			await this.m_ctx.SaveChangesAsync(ct).ConfigureAwait(false);

			trigger.TriggerActions.Add(action);
		}

		public async Task DeleteAsync(long id, CancellationToken ct = default)
		{
			var entity = await this.m_ctx.Triggers.Where(t => t.ID == id).FirstOrDefaultAsync(ct).ConfigureAwait(false);

			if(entity == null) {
				return;
			}

			this.m_ctx.Triggers.Remove(entity);
			await this.m_ctx.SaveChangesAsync(ct).ConfigureAwait(false);
		}

		public async Task DeleteBySensorAsync(string sensorId, CancellationToken ct = default)
		{
			var entity = this.m_ctx.Triggers.Where(t => t.SensorID == sensorId);

			if(!entity.Any()) {
				return;
			}

			this.m_ctx.Triggers.RemoveRange(entity);
			await this.m_ctx.SaveChangesAsync(ct).ConfigureAwait(false);
		}

		public async Task CreateAsync(Trigger trigger, CancellationToken ct = default)
		{
			await this.m_ctx.Triggers.AddAsync(trigger, ct).ConfigureAwait(false);
			await this.m_ctx.SaveChangesAsync(ct).ConfigureAwait(false);
		}

		public async Task UpdateAsync(Trigger trigger, CancellationToken ct = default)
		{
			var value = await this.m_ctx.Triggers.FirstOrDefaultAsync(t => t.ID == trigger.ID, ct).ConfigureAwait(false);

			value.SensorID = trigger.SensorID;
			value.UpperEdge = trigger.UpperEdge;
			value.LowerEdge = trigger.LowerEdge;
			value.KeyValue = trigger.KeyValue;
			value.FormalLanguage = trigger.FormalLanguage;
			value.Type = trigger.Type;

			this.m_ctx.Triggers.Update(value);
			await this.m_ctx.SaveChangesAsync(ct).ConfigureAwait(false);
		}

		public async Task StoreTriggerInvocations(IEnumerable<TriggerInvocation> invocations, CancellationToken ct)
		{
			await this.m_ctx.TriggerInvocations.AddRangeAsync(invocations, ct).ConfigureAwait(false);
			await this.m_ctx.SaveChangesAsync(ct).ConfigureAwait(false);
		}
	}
}
