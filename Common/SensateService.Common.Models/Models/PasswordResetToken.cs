/*
 * Password reset token bridge.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SensateService.Common.Data.Models
{
	[Table("PasswordResetTokens")]
	public class PasswordResetToken
	{
		[Key]
		public string UserToken { get; set; }
		public string IdentityToken { get; set; }
	}
}
