/*
 * user service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */


using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

using SensateIoT.API.Common.Core.Helpers;
using SensateIoT.API.Common.Core.Infrastructure.Repositories;
using SensateIoT.API.Common.IdentityData.Models;

namespace SensateIoT.API.Common.Core.Services.DataProcessing
{
	public class UserService : IUserService
	{
		private readonly UserManager<SensateUser> m_users;
		private readonly IAuditLogRepository m_logs;

		public UserService(
			UserManager<SensateUser> users,
			IAuditLogRepository logs
		)
		{
			this.m_users = users;
			this.m_logs = logs;
		}

		public async Task DeleteAsync(SensateUser user, CancellationToken ct = default)
		{
			await this.m_users.DeleteAsync(user).AwaitBackground();
			await this.m_logs.DeleteBetweenAsync(user, DateTime.MinValue, DateTime.Now);
		}
	}
}