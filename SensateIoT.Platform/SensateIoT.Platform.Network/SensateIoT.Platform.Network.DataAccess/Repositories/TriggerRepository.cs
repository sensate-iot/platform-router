/*
 * Sensor trigger information.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MongoDB.Bson;
using Npgsql;

using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.DataAccess.Contexts;
using TriggerAction = SensateIoT.Platform.Network.Data.DTO.TriggerAction;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class TriggerRepository : ITriggerRepository
	{
		private readonly TriggerContext m_ctx;

		private const string Router_GetTriggers = "router_gettriggers";
		private const string Router_GetTriggerByID = "router_gettriggersbyid";
		private const string TriggerService_GetTriggersBySensorID = "triggerservice_gettriggersbysensorid";

		public TriggerRepository(TriggerContext ctx)
		{
			this.m_ctx = ctx;
		}

		public async Task<TriggerRoutingInfo> GetTriggerInfoAsync(ObjectId sensorID, CancellationToken ct)
		{
			var result = new TriggerRoutingInfo();

			using(var cmd = this.m_ctx.Database.GetDbConnection().CreateCommand()) {
				if(cmd.Connection.State != ConnectionState.Open) {
					await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
				}

				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = Router_GetTriggerByID;
				cmd.Parameters.Add(new NpgsqlParameter("id", DbType.String) { Value = sensorID.ToString() });

				using(var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false)) {
					if(await reader.ReadAsync(ct)) {
						result = new TriggerRoutingInfo {
							SensorID = ObjectId.Parse(reader.GetString(0)),
							ActionCount = reader.GetInt64(1),
							TextTrigger = reader.GetBoolean(2)
						};
					}
				}
			}

			return result;
		}

		public async Task<IEnumerable<TriggerRoutingInfo>> GetTriggerInfoAsync(CancellationToken ct)
		{
			var result = new List<TriggerRoutingInfo>();

			using(var cmd = this.m_ctx.Database.GetDbConnection().CreateCommand()) {
				if(cmd.Connection.State != ConnectionState.Open) {
					await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
				}

				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = Router_GetTriggers;

				using(var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false)) {
					while(await reader.ReadAsync(ct)) {
						var record = new TriggerRoutingInfo {
							SensorID = ObjectId.Parse(reader.GetString(0)),
							ActionCount = reader.GetInt64(1),
							TextTrigger = reader.GetBoolean(2)
						};

						result.Add(record);
					}
				}
			}

			return result;
		}

		public async Task<IEnumerable<TriggerAction>> GetTriggerServiceActions(IEnumerable<ObjectId> sensorIds, CancellationToken ct)
		{
			var result = new List<TriggerAction>();

			using(var cmd = this.m_ctx.Database.GetDbConnection().CreateCommand()) {
				if(cmd.Connection.State != ConnectionState.Open) {
					await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
				}

				var idArray = string.Join(",", sensorIds);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = TriggerService_GetTriggersBySensorID;
				cmd.Parameters.Add(new NpgsqlParameter("idlist", DbType.String) { Value = idArray });

				using(var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false)) {
					while(await reader.ReadAsync(ct)) {
						var record = new TriggerAction {
							TriggerID = reader.GetInt64(0),
							ActionID = reader.GetInt64(1),
							SensorID = ObjectId.Parse(reader.GetString(2)),
							KeyValue = reader.GetString(3),
							LowerEdge = !reader.IsDBNull(4) ? reader.GetDecimal(4) : 0,
							UpperEdge = !reader.IsDBNull(5) ? reader.GetDecimal(5) : 0,
							FormalLanguage = !reader.IsDBNull(6) ? reader.GetString(6) : null,
							Type = (TriggerType) reader.GetFieldValue<int>(7),
							Channel = (TriggerChannel) reader.GetFieldValue<int>(8),
							Target = !reader.IsDBNull(9)? reader.GetString(9): null,
							Message = !reader.IsDBNull(10) ? reader.GetString(10) : null,
							LastInvocation = !reader.IsDBNull(11) ? reader.GetDateTime(11) : DateTime.MinValue
						};

						result.Add(record);
					}
				}
			}

			return result;
		}
	}
}
