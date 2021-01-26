/*
 * Change email token repository interface.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using SensateIoT.API.Common.Data.Models;

namespace SensateIoT.API.Common.Core.Infrastructure.Repositories
{
	public interface IChangeEmailTokenRepository
	{
		void Create(ChangeEmailToken token);
		string Create(string token, string email);
		ChangeEmailToken GetById(string id);
	}
}
