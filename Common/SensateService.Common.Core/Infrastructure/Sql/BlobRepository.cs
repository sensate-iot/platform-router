/*
 * Blob data access layer.
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

using Npgsql;
using NpgsqlTypes;

using SensateService.Common.Data.Enums;
using SensateService.Common.Data.Models;
using SensateService.Infrastructure.Repositories;

namespace SensateService.Infrastructure.Sql
{
	public class BlobRepository : IBlobRepository
	{
		private readonly NetworkContext m_ctx;

		private const string StatsGetBlobs = "StatsService_GetBlobs";

		public BlobRepository(NetworkContext context)
		{
			this.m_ctx = context;
		}

		public async Task<IEnumerable<Blob>> GetAsync(IList<Sensor> sensors,
													  DateTime start,
													  DateTime end,
													  int skip = -1,
													  int limit = -1,
													  OrderDirection order = OrderDirection.Ascending,
													  CancellationToken ct = default)
		{
			var rv = new List<Blob>();
			var ids = sensors.Select(x => x.InternalId.ToString());
			var sensoridlist = string.Join(',', ids);

			await using var cmd = this.m_ctx.Database.GetDbConnection().CreateCommand();

			if(cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			cmd.CommandType = System.Data.CommandType.StoredProcedure;
			cmd.CommandText = StatsGetBlobs;

			var idlst = new NpgsqlParameter("idlist", NpgsqlDbType.Text) { Value = sensoridlist };
			var _start = new NpgsqlParameter("start", NpgsqlDbType.Timestamp) { Value = start };
			var _end = new NpgsqlParameter("end", NpgsqlDbType.Timestamp) { Value = end };
			var _skip = new NpgsqlParameter("ofst", NpgsqlDbType.Integer) { Value = GetNullableInteger(skip) };
			var _limit = new NpgsqlParameter("lim", NpgsqlDbType.Integer) { Value = GetNullableInteger(limit) };
			var _direction = new NpgsqlParameter("direction", NpgsqlDbType.Varchar) { Value = order.ToString("G") };

			cmd.Parameters.Add(idlst);
			cmd.Parameters.Add(_start);
			cmd.Parameters.Add(_end);
			cmd.Parameters.Add(_skip);
			cmd.Parameters.Add(_limit);
			cmd.Parameters.Add(_direction);

			await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

			if(!reader.HasRows) {
				return null;
			}

			while(await reader.ReadAsync(ct).ConfigureAwait(false)) {
				var tmp = new Blob {
					Id = reader.GetInt64(0),
					SensorId = reader.GetString(1),
					FileName = reader.GetString(2),
					Path = reader.GetString(3),
					StorageType = (StorageType)reader.GetInt64(4),
					Timestamp = reader.GetDateTime(5),
					FileSize = reader.GetInt64(6)
				};

				rv.Add(tmp);
			}

			return rv;
		}

		private static int? GetNullableInteger(int value)
		{
			if(value < 0) {
				return null;
			}

			return value;
		}
	}
}
