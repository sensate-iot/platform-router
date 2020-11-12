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

using Microsoft.EntityFrameworkCore;

using MongoDB.Bson;
using Npgsql;

using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Contexts;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class TriggerRepository : ITriggerRepository
	{
		private readonly TriggerContext m_ctx;

		private const string Router_GetTriggers = "router_gettriggers";
		private const string Router_GetTriggerByID = "router_gettriggersbyid";

		public TriggerRepository(TriggerContext ctx)
		{
			this.m_ctx = ctx;
		}

		public async Task<IEnumerable<RoutingTriggerInfo>> GetTriggerInfoAsync(ObjectId sensorID, CancellationToken ct)
		{
			var result = new List<RoutingTriggerInfo>();

			using(var cmd = this.m_ctx.Database.GetDbConnection().CreateCommand()) {
				if(cmd.Connection.State != ConnectionState.Open) {
					await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
				}

				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = Router_GetTriggerByID;
				cmd.Parameters.Add(new NpgsqlParameter("id", DbType.String) { Value = sensorID.ToString() });

				using(var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false)) {
					while(await reader.ReadAsync(ct)) {
						var info = new RoutingTriggerInfo {
							SensorID = ObjectId.Parse(reader.GetString(0)),
							ActionCount = reader.GetInt64(1),
							TextTrigger = reader.GetBoolean(2)
						};

						result.Add(info);
					}
				}
			}

			return result;
		}

		public async Task<IEnumerable<RoutingTriggerInfo>> GetTriggerInfoAsync(CancellationToken ct)
		{
			var result = new List<RoutingTriggerInfo>();

			using(var cmd = this.m_ctx.Database.GetDbConnection().CreateCommand()) {
				if(cmd.Connection.State != ConnectionState.Open) {
					await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
				}

				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = Router_GetTriggers;

				using(var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false)) {
					while(await reader.ReadAsync(ct)) {
						var record = new RoutingTriggerInfo {
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
	}
}
