/*
 * Password reset token repository.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Threading.Tasks;

using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IPasswordResetTokenRepository
	{
		void Create(PasswordResetToken token);
		string Create(string token);
		PasswordResetToken GetById(string id);
	}
}
