/*
 * API key data access implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Data;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Npgsql;
using NpgsqlTypes;

using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Contexts;
using SensateService.Common.IdentityData.Enums;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class ApiKeyRepository : IApiKeyRepository
	{
		private readonly AuthorizationContext m_ctx;
		private const string NetworkApi_GetApiKeyByKey = "networkapi_selectapikeybykey";

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
				Type = (ApiKeyType) reader.GetInt32(2),
				ReadOnly = reader.GetBoolean(3)
			};

			return apikey;
		}
	}
}
