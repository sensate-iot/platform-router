/*
 * Password reset token bridge.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Models
{
	public class PasswordResetToken
	{
		[Key]
		public string UserToken { get; set; }
		public string IdentityToken { get; set; }

		public PasswordResetToken()
		{
		}
	}
}
