/*
 * User repository interface.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System.Threading.Tasks;
using System.Collections.Generic;
using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IUserRepository
	{
		SensateUser Get(string key);
		Task<SensateUser> GetAsync(string key);
	}
}
