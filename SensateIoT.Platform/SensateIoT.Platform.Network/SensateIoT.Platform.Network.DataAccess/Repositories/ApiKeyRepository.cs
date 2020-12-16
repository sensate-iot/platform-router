/*
 * API key data access implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Npgsql;
using NpgsqlTypes;

using SensateIoT.Platform.Network.Data.Enums;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Contexts;

using CommandType = System.Data.CommandType;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class ApiKeyRepository : IApiKeyRepository
	{
		private readonly AuthorizationContext m_ctx;
		private readonly Random m_random;

		private const string NetworkApi_GetApiKeyByKey = "networkapi_selectapikeybykey";
		private const string NetworkApi_DeleteSensorKey = "networkapi_deletesensorkey";

		public ApiKeyRepository(AuthorizationContext ctx)
		{
			this.m_ctx = ctx;
			this.m_random = new Random(DateTime.UtcNow.Millisecond * DateTime.UtcNow.Second);
		}

		public async Task<ApiKey> GetAsync(string key, CancellationToken ct = default)
		{
			ApiKey apikey;

			await using var cmd = this.m_ctx.Database.GetDbConnection().CreateCommand();
			if(cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = NetworkApi_GetApiKeyByKey;

			var param = new NpgsqlParameter("key", NpgsqlDbType.Text) { Value = key };
			cmd.Parameters.Add(param);

			await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

			if(!reader.HasRows) {
				return null;
			}

			await reader.ReadAsync(ct).ConfigureAwait(false);

			apikey = new ApiKey {
				Key = key,
				UserId = reader.GetGuid(0),
				Revoked = reader.GetBoolean(1),
				Type = (ApiKeyType)reader.GetInt32(2),
				ReadOnly = reader.GetBoolean(3)
			};

			return apikey;
		}

		public async Task DeleteAsync(string key, CancellationToken ct = default)
		{
			await using var cmd = this.m_ctx.Database.GetDbConnection().CreateCommand();
			if(cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = NetworkApi_DeleteSensorKey;

			var param = new NpgsqlParameter("key", NpgsqlDbType.Text) { Value = key };
			cmd.Parameters.Add(param);

			await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
		}

		public async Task<ApiKey> CreateSensorKeyAsync(Sensor sensor, CancellationToken ct = default)
		{
			ApiKey apikey;

			await using var cmd = this.m_ctx.Database.GetDbConnection().CreateCommand();
			if(cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = NetworkApi_GetApiKeyByKey;

			var key = new NpgsqlParameter("key", NpgsqlDbType.Text) { Value = sensor.Secret };
			var userId = new NpgsqlParameter("userId", NpgsqlDbType.Text) { Value = sensor.Owner };

			cmd.Parameters.Add(key);
			cmd.Parameters.Add(userId);

			await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

			if(!reader.HasRows) {
				return null;
			}

			await reader.ReadAsync(ct).ConfigureAwait(false);

			apikey = new ApiKey {
				UserId = reader.GetGuid(1),
				Key = reader.GetString(2),
				Revoked = reader.GetBoolean(3),
				Type = (ApiKeyType)reader.GetInt32(4),
				ReadOnly = reader.GetBoolean(5)
			};

			return apikey;
		}

		public async Task UpdateAsync(string old, string @new, CancellationToken ct = default)
		{
			throw new NotImplementedException();
		}
	}
}
