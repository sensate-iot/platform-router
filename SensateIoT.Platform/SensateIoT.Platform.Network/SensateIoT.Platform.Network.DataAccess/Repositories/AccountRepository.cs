/*
 * Account repository implementation.
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

using Npgsql;
using NpgsqlTypes;

using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Contexts;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class AccountRepository : IAccountRepository
	{
		private const string NetworkApi_GetAccountByID = "networkapi_selectuserbyid";
		private const string NetworkApi_GetAccountsByID = "networkapi_selectusersbyid";

		private readonly AuthorizationContext m_ctx;

		public AccountRepository(AuthorizationContext ctx)
		{
			this.m_ctx = ctx;
		}

		public async Task<User> GetAccountAsync(Guid accountGuid, CancellationToken ct = default)
		{
			User user;

			await using var cmd = this.m_ctx.Database.GetDbConnection().CreateCommand();
			if(cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = NetworkApi_GetAccountByID;

			var param = new NpgsqlParameter("userid", NpgsqlDbType.Uuid) { Value = accountGuid };
			cmd.Parameters.Add(param);

			await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

			if(!reader.HasRows) {
				return null;
			}

			await reader.ReadAsync(ct).ConfigureAwait(false);

			user = new User {
				ID = reader.GetGuid(0),
				FirstName = reader.GetString(1),
				LastName = reader.GetString(2),
				Email = reader.GetString(3),
				RegisteredAt = reader.GetDateTime(4),
				PhoneNumber = reader.GetString(5),
				BillingLockout = reader.GetBoolean(6),
				UserRoles = new List<string> { reader.GetString(7) }
			};

			while(await reader.ReadAsync(ct)) {
				user.UserRoles.Add(reader.GetString(7));
			}

			return user;
		}

		public async Task<IEnumerable<User>> GetAccountsAsync(IEnumerable<string> idlist, CancellationToken ct = default)
		{
			var users = new List<User>();

			await using var cmd = this.m_ctx.Database.GetDbConnection().CreateCommand();
			if(cmd.Connection.State != ConnectionState.Open) {
				await cmd.Connection.OpenAsync(ct).ConfigureAwait(false);
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = NetworkApi_GetAccountsByID;

			var idArray = string.Join(",", idlist);
			var param = new NpgsqlParameter("userids", NpgsqlDbType.Text) { Value = idArray };
			cmd.Parameters.Add(param);

			await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

			if(!reader.HasRows) {
				return null;
			}

			User user = null;

			while(await reader.ReadAsync(ct).ConfigureAwait(false)) {

				var tmp = new User {
					ID = reader.GetGuid(0),
				};

				if(user?.ID == tmp.ID) {
					user.UserRoles.Add(reader.GetString(7));
					continue;
				}

				tmp.FirstName = reader.GetString(1);
				tmp.LastName = reader.GetString(2);
				tmp.Email = reader.GetString(3);
				tmp.RegisteredAt = reader.GetDateTime(4);
				tmp.PhoneNumber = reader.GetString(5);
				tmp.BillingLockout = reader.GetBoolean(6);
				tmp.UserRoles = new List<string> { reader.GetString(7) };

				user = tmp;
				users.Add(tmp);
			}


			return users;
		}
	}
}
