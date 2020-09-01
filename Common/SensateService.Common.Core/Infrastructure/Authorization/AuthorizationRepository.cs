/*
 * Data authorization context repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MongoDB.Bson;
using MongoDB.Driver;

using SensateService.Common.Data.Dto.Authorization;
using SensateService.Helpers;
using SensateService.Infrastructure.Document;
using SensateService.Infrastructure.Repositories;
using SensateService.Infrastructure.Sql;

namespace SensateService.Infrastructure.Authorization
{
	public class AuthorizationRepository : IAuthorizationRepository
	{
		private const string KeyFunction = "authorizationctx_getapikeys";
		private const string UserFunction = "authorizationctx_getuseraccounts";

		private readonly IMongoCollection<Common.Data.Models.Sensor> m_sensors;
		private readonly SensateSqlContext m_sql;

		public AuthorizationRepository(SensateContext mongoCtx, SensateSqlContext sqlCtx)
		{
			this.m_sensors = mongoCtx.Sensors;
			this.m_sql = sqlCtx;
		}

		public async Task<IEnumerable<User>> GetAllUsersAsync()
		{
			var rv = new List<User>();

			await using var cmd = this.m_sql.Database.GetDbConnection().CreateCommand();
			if(cmd.Connection.State != ConnectionState.Open) {
				cmd.Connection.Open();
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = UserFunction;
			await using var reader = await cmd.ExecuteReaderAsync().AwaitBackground();
			while(await reader.ReadAsync()) {
				var id = reader.GetString(0);
				var billing = reader.GetBoolean(1);
				var banned = reader.GetBoolean(2);

				rv.Add(new User {
					Banned = banned,
					BillingLockout = billing,
					Id = id
				});
			}

			return rv;
		}

		public async Task<IEnumerable<Sensor>> GetAllSensorsAsync()
		{
			var query =
				this.m_sensors.Aggregate()
					.Project<Sensor>(new BsonDocument {
						{ "Secret", 1 },
						{ "UserId", "$Owner" },
					});

			return await query.ToListAsync().AwaitBackground();
		}

		public async Task<IEnumerable<ApiKey>> GetAllSensorKeysAsync()
		{
			var rv = new List<ApiKey>();

			await using var cmd = this.m_sql.Database.GetDbConnection().CreateCommand();
			if(cmd.Connection.State != ConnectionState.Open) {
				cmd.Connection.Open();
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = KeyFunction;
			await using var reader = await cmd.ExecuteReaderAsync().AwaitBackground();
			while(await reader.ReadAsync()) {
				var key = reader.GetString(0);
				var revoked = reader.GetBoolean(1);
				var @readonly = reader.GetBoolean(2);

				rv.Add(new ApiKey {
					Key = key,
					ReadOnly = @readonly,
					Revoked = revoked
				});
			}

			return rv;
		}
	}
}