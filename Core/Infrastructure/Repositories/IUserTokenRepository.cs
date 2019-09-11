/*
 * Repository for the SensateUserToken table.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IUserTokenRepository
	{
		void Create(UserToken token);
		Task CreateAsync(UserToken token, CancellationToken ct = default(CancellationToken));

		Task<long> CountAsync(Expression<Func<UserToken, bool>> expr);

		IEnumerable<UserToken> GetByUser(SensateUser user);
		UserToken GetById(SensateUser user, string value);
		UserToken GetById(Tuple<SensateUser, string> id);

		string GenerateJwtToken(SensateUser user, IEnumerable<string> roles, UserAccountSettings settings);
		string GenerateRefreshToken();

		void InvalidateToken(UserToken token);
		void InvalidateToken(SensateUser user, string value);

		Task InvalidateTokenAsync(UserToken token);
		Task InvalidateManyAsync(IEnumerable<UserToken> tokens);
		Task InvalidateTokenAsync(SensateUser user, string value);
	}
}
