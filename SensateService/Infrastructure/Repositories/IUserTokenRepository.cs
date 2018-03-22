/*
 * Repository for the SensateUserToken table.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SensateService.Controllers;
using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IUserTokenRepository
	{
		void Create(UserToken token);
		Task CreateAsync(UserToken token);

		IEnumerable<UserToken> GetByUser(SensateUser user);
		UserToken GetById(SensateUser user, string value);
		UserToken GetById(Tuple<SensateUser, string> id);

		string GenerateJwtToken(SensateUser user, IEnumerable<string> roles, UserAccountSettings settings);
		string GenerateRefreshToken();

		void InvalidateToken(UserToken token);
		void InvalidateToken(SensateUser user, string value);

		Task InvalidateTokenAsync(UserToken token);
		Task InvalidateTokenAsync(SensateUser user, string value);
	}
}
