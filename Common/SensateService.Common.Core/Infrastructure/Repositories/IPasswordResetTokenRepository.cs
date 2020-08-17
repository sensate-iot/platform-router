/*
 * Password reset token repository.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using SensateService.Common.Data.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IPasswordResetTokenRepository
	{
		void Create(PasswordResetToken token);
		string Create(string token);
		PasswordResetToken GetById(string id);
	}
}
