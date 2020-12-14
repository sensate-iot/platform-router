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
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Contexts;

using TriggerAction = SensateIoT.Platform.Network.Data.DTO.TriggerAction;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class TriggerRepository : ITriggerRepository
	{
		private readonly TriggerContext m_ctx;

		private const string TriggerService_GetTriggersBySensorID = "triggerservice_gettriggersbysensorid";

		public TriggerRepository(TriggerContext ctx)
		{
			this.m_ctx = ctx;
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
							FormalLanguage = !reader.IsDBNull(6) ? reader.GetString(6) : null,
							Type = (TriggerType) reader.GetFieldValue<int>(7),
							Channel = (TriggerChannel) reader.GetFieldValue<int>(8),
							Target = !reader.IsDBNull(9)? reader.GetString(9): null,
							Message = !reader.IsDBNull(10) ? reader.GetString(10) : null,
							LastInvocation = !reader.IsDBNull(11) ? reader.GetDateTime(11) : DateTime.MinValue
						};

						if(reader.IsDBNull(4)) {
							record.LowerEdge = null;
						} else {
							record.LowerEdge = reader.GetDecimal(4);
						}

						if(reader.IsDBNull(5)) {
							record.UpperEdge = null;
						} else {
							record.UpperEdge = reader.GetDecimal(5);
						}

						result.Add(record);
					}
				}
			}

			return result;
		}

		public async Task StoreTriggerInvocation(TriggerInvocation invocation, CancellationToken ct)
		{
			this.m_ctx.TriggerInvocations.Add(invocation);
			await this.m_ctx.SaveChangesAsync(ct).ConfigureAwait(false);
		}

		public async Task StoreTriggerInvocations(IEnumerable<TriggerInvocation> invocations, CancellationToken ct)
		{
			await this.m_ctx.TriggerInvocations.AddRangeAsync(invocations, ct).ConfigureAwait(false);
			await this.m_ctx.SaveChangesAsync(ct).ConfigureAwait(false);
		}
	}
}
