/*
 * Sensor trigger information.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;
using Npgsql;
using NpgsqlTypes;

using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.DataAccess.Abstract;

using TriggerAction = SensateIoT.Platform.Network.Data.DTO.TriggerAction;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class TriggerRepository : ITriggerRepository
	{
		private readonly INetworkingDbContext m_ctx;

		private const string TriggerService_GetTriggersBySensorID = "triggerservice_gettriggersbysensorid";
		private const string TriggerService_GetTriggers = "triggerservice_gettriggers";

		public TriggerRepository(INetworkingDbContext ctx)
		{
			this.m_ctx = ctx;
		}

		public async Task<IEnumerable<TriggerAction>> GetTriggerServiceActions(CancellationToken ct)
		{
			var result = new List<TriggerAction>();

			await using var cmd = this.m_ctx.Connection.CreateCommand();

			if(cmd.Connection != null && cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = TriggerService_GetTriggers;

			await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
			while(await reader.ReadAsync(ct)) {
				var record = new TriggerAction {
					TriggerID = reader.GetInt64(0),
					ActionID = reader.GetInt64(1),
					SensorID = ObjectId.Parse(reader.GetString(2)),
					KeyValue = reader.GetString(3),
					FormalLanguage = !await reader.IsDBNullAsync(6, ct) ? reader.GetString(6) : null,
					Type = (TriggerType)reader.GetFieldValue<int>(7),
					Channel = (TriggerChannel)reader.GetFieldValue<int>(8),
					Target = !await reader.IsDBNullAsync(9, ct) ? reader.GetString(9) : null,
					Message = !await reader.IsDBNullAsync(10, ct) ? reader.GetString(10) : null
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

		public async Task<IEnumerable<TriggerAction>> GetTriggerServiceActionsBySensorId(ObjectId id, CancellationToken ct = default)
		{
			var result = new List<TriggerAction>();

			await using var cmd = this.m_ctx.Connection.CreateCommand();

			if(cmd.Connection != null && cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = TriggerService_GetTriggersBySensorID;
			var param = new NpgsqlParameter("id", NpgsqlDbType.Varchar) {
				Value = id.ToString()
			};
			cmd.Parameters.Add(param);

			await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
			while(await reader.ReadAsync(ct)) {
				var record = new TriggerAction {
					TriggerID = reader.GetInt64(0),
					ActionID = reader.GetInt64(1),
					SensorID = ObjectId.Parse(reader.GetString(2)),
					KeyValue = reader.GetString(3),
					FormalLanguage = !await reader.IsDBNullAsync(6, ct) ? reader.GetString(6) : null,
					Type = (TriggerType)reader.GetFieldValue<int>(7),
					Channel = (TriggerChannel)reader.GetFieldValue<int>(8),
					Target = !await reader.IsDBNullAsync(9, ct) ? reader.GetString(9) : null,
					Message = !await reader.IsDBNullAsync(10, ct) ? reader.GetString(10) : null
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
	}
}
