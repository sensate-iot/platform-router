/*
 * User repository interface.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using SensateService.Models;
using SensateService.Models.Json.Out;

namespace SensateService.Infrastructure.Repositories
{
	public interface IUserRepository
	{
		SensateUser Get(string key);
		Task<SensateUser> GetAsync(string key);
		Task<SensateUser> GetByEmailAsync(string key, CancellationToken ct = default);
		Task<IEnumerable<SensateUser>> GetRangeAsync(IEnumerable<string> ids);
		Task<IEnumerable<SensateUser>> GetAllAsync(int skip = 0, int limit = 0, CancellationToken ct = default);

		Task<IEnumerable<SensateUser>> FindByEmailAsync(string email, int skip = 0, int limit = 0, CancellationToken ct = default);
		Task<int> CountFindAsync(string email, CancellationToken ct = default);

		Task<SensateUser> GetByClaimsPrincipleAsync(ClaimsPrincipal cp);

		void StartUpdate(SensateUser user);
		Task EndUpdateAsync(CancellationToken token = default(CancellationToken));
		IEnumerable<string> GetRoles(SensateUser user);
		Task<IEnumerable<string>> GetRolesAsync(SensateUser user);

		Task DeleteAsync(string id, CancellationToken ct = default);

		Task<int> CountGhostUsersAsync();
		Task<int> CountAsync();
		Task<List<Tuple<DateTime, int>>> CountByDay(DateTime start);

		Task<PaginationResult<SensateUser>> GetMostRecentAsync(int skip = 0, int limit = 0, CancellationToken ct = default);

		Task<bool> IsBanned(SensateUser user);
		Task<bool> IsAdministrator(SensateUser user);
		Task<bool> ClearRolesForAsync(SensateUser user);
		Task<bool> AddToRolesAsync(SensateUser user, IEnumerable<string> roles);
	}
}
