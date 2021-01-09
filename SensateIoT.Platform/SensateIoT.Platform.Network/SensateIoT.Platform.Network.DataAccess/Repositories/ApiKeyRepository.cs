/*
 * API key data access implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;
using System.Data;
using System.Threading;

using Microsoft.EntityFrameworkCore;

using Npgsql;
using NpgsqlTypes;

using SensateIoT.Platform.Network.Data.Enums;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Contexts;

using CommandType = System.Data.CommandType;
using SensateIoT.Platform.Network.DataAccess.Extensions;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class ApiKeyRepository : IApiKeyRepository
	{
		private readonly AuthorizationContext m_ctx;

		private const string NetworkApi_GetApiKeyByKey = "networkapi_selectapikeybykey";
		private const string NetworkApi_DeleteSensorKey = "networkapi_deletesensorkey";
		private const string NetworkApi_UpdateApiKey = "networkapi_updateapikey";
		private const string NetworkApi_IncrementRequestCount = "networkapi_incrementrequestcount";
		private const string NetworkApi_CreateSensorKey = "networkapi_createsensorkey";

		public ApiKeyRepository(AuthorizationContext ctx)
		{
			this.m_ctx = ctx;
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
			cmd.CommandText = NetworkApi_CreateSensorKey;

			var key = new NpgsqlParameter("key", NpgsqlDbType.Text) { Value = sensor.Secret };
			var uid = new NpgsqlParameter("userid", NpgsqlDbType.Text) { Value = sensor.Owner };

			cmd.Parameters.Add(key);
			cmd.Parameters.Add(uid);

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

		public async Task<ApiKey> UpdateAsync(string old, string @new, CancellationToken ct = default)
		{
			ApiKey apikey;

			await using var cmd = this.m_ctx.Database.GetDbConnection().CreateCommand();
			if(cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = NetworkApi_UpdateApiKey;

			var oldArgument = new NpgsqlParameter("old", NpgsqlDbType.Text) { Value = old };
			var newArgument = new NpgsqlParameter("new", NpgsqlDbType.Text) { Value = @new };

			cmd.Parameters.Add(oldArgument);
			cmd.Parameters.Add(newArgument);

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

		public async Task IncrementRequestCountAsync(string key, CancellationToken ct = default)
		{
			using var builder = StoredProcedureBuilder.Create(this.m_ctx.Database.GetDbConnection());

			builder.WithParameter("key", key, NpgsqlDbType.Text);
			builder.WithFunction(NetworkApi_IncrementRequestCount);

			var reader = await builder.ExecuteAsync(ct).ConfigureAwait(false);
			await reader.DisposeAsync().ConfigureAwait(false);
		}
	}
}
