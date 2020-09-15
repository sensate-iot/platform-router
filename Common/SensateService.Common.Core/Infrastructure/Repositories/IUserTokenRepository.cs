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

using SensateService.Common.Config.Settings;
using SensateService.Common.IdentityData.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IUserTokenRepository
	{
		void Create(AuthUserToken token);
		Task CreateAsync(AuthUserToken token, CancellationToken ct = default(CancellationToken));

		Task<long> CountAsync(Expression<Func<AuthUserToken, bool>> expr);

		Task<IEnumerable<AuthUserToken>> GetByUserAsync(SensateUser user, int skip = 0, int limit = 0, CancellationToken ct = default);
		AuthUserToken GetById(SensateUser user, string value);
		AuthUserToken GetById(Tuple<SensateUser, string> id);

		string GenerateJwtToken(SensateUser user, IEnumerable<string> roles, UserAccountSettings settings);
		string GenerateRefreshToken();

		void InvalidateToken(AuthUserToken token);
		void InvalidateToken(SensateUser user, string value);

		Task InvalidateTokenAsync(AuthUserToken token);
		Task InvalidateManyAsync(IEnumerable<AuthUserToken> tokens);
		Task InvalidateTokenAsync(SensateUser user, string value);
	}
}
