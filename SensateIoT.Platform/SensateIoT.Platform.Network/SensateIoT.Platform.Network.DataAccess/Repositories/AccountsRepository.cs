/*
 * Account repository interface.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.DataAccess.Contexts;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class AccountsRepository : IAccountsRepository
	{
		private readonly AuthorizationContext m_ctx;

		private const string Router_GetAccounts = "router_getaccounts";
		private const string Router_GetSensorKeys = "router_getsensorkeys";

		public AccountsRepository(AuthorizationContext ctx)
		{
			this.m_ctx = ctx;
		}

		public async Task<IEnumerable<Account>> GetAccountsAsync(CancellationToken ct)
		{
			var result = new List<Account>();

			using(var cmd = this.m_ctx.Database.GetDbConnection().CreateCommand()) {
				if(cmd.Connection.State != ConnectionState.Open) {
					await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
				}

				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = Router_GetAccounts;

				using(var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false)) {
					while(await reader.ReadAsync(ct)) {
						var account = new Account {
							ID = reader.GetGuid(0),
							HasBillingLockout = reader.GetBoolean(1),
							IsBanned = reader.GetBoolean(2)
						};

						result.Add(account);
					}
				}
			}

			return result;
		}

		public async Task<IEnumerable<ApiKey>> GetApiKeysAsync(CancellationToken ct = default)
		{
			var result = new List<ApiKey>();

			using(var cmd = this.m_ctx.Database.GetDbConnection().CreateCommand()) {
				if(cmd.Connection.State != ConnectionState.Open) {
					await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
				}

				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = Router_GetSensorKeys;

				using(var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false)) {
					while(await reader.ReadAsync(ct)) {
						var key = new ApiKey {
							Key = reader.GetString(0),
							AccountID = reader.GetGuid(1),
							IsRevoked = reader.GetBoolean(2),
							IsReadOnly = reader.GetBoolean(3)
						};

						result.Add(key);
					}
				}
			}

			return result;

		}
	}
}
