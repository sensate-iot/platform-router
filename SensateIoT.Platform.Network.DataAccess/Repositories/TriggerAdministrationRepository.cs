/*
 * Sensor trigger information.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Npgsql;
using NpgsqlTypes;

using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Extensions;

using TriggerAction = SensateIoT.Platform.Network.Data.Models.TriggerAction;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class TriggerAdministrationRepository : ITriggerAdministrationRepository
	{
		private readonly INetworkingDbContext m_ctx;

		private const string NetworkApi_CreateTrigger = "networkapi_createtrigger";
		private const string NetworkApi_CreateAction = "networkapi_createaction";
		private const string NetworkApi_DeleteTriggersBySensorID = "networkapi_deletetriggersbysensorid";
		private const string NetworkApi_DeleteTriggerByID = "networkapi_deletetriggerbyid";
		private const string NetworkApi_SelectTriggerByID = "networkapi_selecttriggerbyid";
		private const string NetworkApi_SelectTriggersBySensorID = "networkapi_selecttriggerbysensorid";
		private const string NetworkApi_DeleteTriggerAction = "networkapi_deletetriggeraction";

		public TriggerAdministrationRepository(INetworkingDbContext ctx)
		{
			this.m_ctx = ctx;
		}

		public async Task<IEnumerable<Trigger>> GetAsync(string sensorId, TriggerType type, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Connection);

			builder.WithParameter("sensorid", sensorId, NpgsqlDbType.Varchar);
			builder.WithFunction(NetworkApi_SelectTriggersBySensorID);

			await using var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);

			var result = await GetTriggersFromReaderAsync(reader, ct).ConfigureAwait(false);
			return result.Where(x => x.Type == type);
		}

		public async Task<Trigger> GetAsync(long id, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Connection);

			builder.WithParameter("id", id, NpgsqlDbType.Bigint);
			builder.WithFunction(NetworkApi_SelectTriggerByID);

			await using var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);

			if(await reader.ReadAsync(ct)) {
				var trigger = new Trigger {
					ID = id
				};

				await GetTriggerFromReaderAsync(trigger, reader, ct).ConfigureAwait(false);
				return trigger;
			}

			return null;
		}

		private static async Task<IList<Trigger>> GetTriggersFromReaderAsync(DbDataReader reader, CancellationToken ct)
		{
			var triggers = new List<Trigger>();
			var done = await reader.ReadAsync(ct).ConfigureAwait(false);

			done = !done;

			while(!done) {
				var trigger = new Trigger();
				done = await GetTriggerFromReaderAsync(trigger, reader, ct).ConfigureAwait(false);
				triggers.Add(trigger);
			}

			return triggers;
		}

		private static async Task<bool> GetTriggerFromReaderAsync(Trigger trigger, DbDataReader reader, CancellationToken ct)
		{
			var id = reader.GetInt64(0);

			trigger.ID = id;
			trigger.SensorID = reader.GetString(1);
			trigger.KeyValue = reader.GetString(2);
			trigger.LowerEdge = SafeGetValue<double?>(reader, 3);
			trigger.UpperEdge = SafeGetValue<double?>(reader, 4);
			trigger.FormalLanguage = SafeGetValue<string>(reader, 5);
			trigger.Type = (TriggerType)reader.GetInt32(6);

			do {
				id = reader.GetInt64(0);

				if(trigger.ID != id) {
					return false;
				}

				if(await reader.IsDBNullAsync(7, ct)) {
					var hasNext = await reader.ReadAsync(ct).ConfigureAwait(false);
					return !hasNext;
				}

				var action = new TriggerAction {
					ID = reader.GetInt64(7),
					Channel = (TriggerChannel)reader.GetInt32(8),
					Target = SafeGetValue<string>(reader, 9),
					Message = reader.GetString(10),
					TriggerID = id
				};

				trigger.TriggerActions ??= new List<TriggerAction>();
				trigger.TriggerActions.Add(action);
			} while(await reader.ReadAsync(ct));

			return true;
		}

		private static ColumnValue SafeGetValue<ColumnValue>(DbDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal) ? default : reader.GetFieldValue<ColumnValue>(ordinal);
		}

		public async Task RemoveActionAsync(Trigger trigger, TriggerChannel channel, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Connection);

			builder.WithParameter("triggerid", trigger.ID, NpgsqlDbType.Bigint);
			builder.WithParameter("channel", (int)channel, NpgsqlDbType.Integer);
			builder.WithFunction(NetworkApi_DeleteTriggerAction);

			await using var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);
			var result = await reader.ReadAsync(ct).ConfigureAwait(false);

			if(!result) {
				throw new ArgumentException("Unable to remove non-existing trigger action.");
			}
		}

		public async Task AddActionsAsync(Trigger trigger, IEnumerable<TriggerAction> actions, CancellationToken ct = default)
		{
			foreach(var action in actions) {
				await this.AddActionAsync(trigger, action, ct).ConfigureAwait(false);
			}
		}

		public async Task AddActionAsync(Trigger trigger, TriggerAction action, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Connection);

			builder.WithParameter("triggerid", trigger.ID, NpgsqlDbType.Bigint);
			builder.WithParameter("channel", (int)action.Channel, NpgsqlDbType.Integer);
			builder.WithParameter("target", action.Target, NpgsqlDbType.Varchar);
			builder.WithParameter("message", action.Message, NpgsqlDbType.Text);
			builder.WithFunction(NetworkApi_CreateAction);

			try {
				var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);

				if(await reader.ReadAsync(ct).ConfigureAwait(false)) {
					action.ID = reader.GetInt64(0);
				}

				await reader.DisposeAsync().ConfigureAwait(false);
			} catch(PostgresException exception) {
				if(exception.SqlState == PostgresErrorCodes.UniqueViolation) {
					throw new FormatException("Trigger action for this channel already exists!");
				}
			}
		}

		public async Task DeleteAsync(long id, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Connection);

			builder.WithParameter("id", id, NpgsqlDbType.Bigint);
			builder.WithFunction(NetworkApi_DeleteTriggerByID);

			var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);
			await reader.DisposeAsync().ConfigureAwait(false);
		}

		public async Task DeleteBySensorAsync(string sensorId, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Connection);

			builder.WithParameter("sensorid", sensorId, NpgsqlDbType.Varchar);
			builder.WithFunction(NetworkApi_DeleteTriggersBySensorID);

			var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);
			await reader.DisposeAsync().ConfigureAwait(false);
		}

		public async Task CreateAsync(Trigger trigger, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Connection);

			builder.WithParameter("sensorid", trigger.SensorID, NpgsqlDbType.Varchar);
			builder.WithParameter("keyvalue", trigger.KeyValue, NpgsqlDbType.Varchar);
			builder.WithParameter("loweredge", trigger.LowerEdge, NpgsqlDbType.Numeric);
			builder.WithParameter("upperedge", trigger.UpperEdge, NpgsqlDbType.Numeric);
			builder.WithParameter("formallanguage", trigger.FormalLanguage, NpgsqlDbType.Text);
			builder.WithParameter("type", (int)trigger.Type, NpgsqlDbType.Integer);
			builder.WithFunction(NetworkApi_CreateTrigger);

			var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);

			if(await reader.ReadAsync(ct).ConfigureAwait(false)) {
				trigger.ID = reader.GetInt64(0);
			}

			await reader.DisposeAsync().ConfigureAwait(false);
		}
	}
}
