/*
 * Change email token repository interface.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using SensateService.Common.Data.Models;
using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IChangeEmailTokenRepository
	{
		void Create(ChangeEmailToken token);
		string Create(string token, string email);
		ChangeEmailToken GetById(string id);
	}
}
