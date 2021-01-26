/*
 * Password reset token repository.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using SensateIoT.API.Common.Data.Models;

namespace SensateIoT.API.Common.Core.Infrastructure.Repositories
{
	public interface IPasswordResetTokenRepository
	{
		void Create(PasswordResetToken token);
		string Create(string token);
		PasswordResetToken GetById(string id);
	}
}
