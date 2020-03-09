/*
 * Identity role repository interface.
 * 
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IUserRoleRepository
	{
		void Create(SensateRole role);
		Task CreateAsync(SensateRole role, CancellationToken ct = default(CancellationToken));
		void Create(string name, string description);
		Task CreateAsync(string name, string description);

		void Delete(string name);
		Task DeleteAsync(string name);

		Task UpdateAsync(string name, SensateRole role);
		void Update(string name, SensateRole role);

		Task<SensateRole> GetByNameAsync(string name);
		SensateRole GetById(string id);
		SensateRole GetByName(string name);
		IEnumerable<SensateUser> GetUsers(string id);
		IEnumerable<string> GetRolesFor(SensateUser user);
		Task<IEnumerable<string>> GetRolesForAsync(SensateUser user);
	}
}
