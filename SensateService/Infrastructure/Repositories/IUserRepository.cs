/*
 * User repository interface.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;

using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IUserRepository
	{
		SensateUser Get(string key);
		Task<SensateUser> GetAsync(string key);
		SensateUser GetByEmail(string key);
		Task<SensateUser> GetByEmailAsync(string key);

		SensateUser GetByClaimsPrinciple(ClaimsPrincipal cp);
		Task<SensateUser> GetByClaimsPrincipleAsync(ClaimsPrincipal cp);

		void StartUpdate(SensateUser user);
		Task EndUpdateAsync();
		IEnumerable<string> GetRoles(SensateUser user);
		Task<IEnumerable<string>> GetRolesAsync(SensateUser user);
	}
}
