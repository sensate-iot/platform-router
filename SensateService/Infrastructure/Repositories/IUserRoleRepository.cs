/*
 * Identity role repository interface.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Collections.Generic;
using System.Threading.Tasks;

using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
    public interface IUserRoleRepository
    {
		void Create(UserRole role);
		Task CreateAsync(UserRole role);
		void Create(string name, string description);
		Task CreateAsync(string name, string description);

		void Delete(string name);
		Task DeleteAsync(string name);

		Task UpdateAsync(string name, UserRole role);
		void Update(string name, UserRole role);

		UserRole GetById(string id);
		UserRole GetByName(string name);
		IEnumerable<SensateUser> GetUsers(string id);
		IEnumerable<string> GetRolesFor(SensateUser user);
		Task<IEnumerable<string>> GetRolesForAsync(SensateUser user);
    }
}
