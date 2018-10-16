/*
 * Change email token repository interface.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

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
