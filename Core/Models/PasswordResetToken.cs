/*
 * Password reset token bridge.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SensateService.Models
{
	[Table("AspNetPasswordResetTokens")]
	public class PasswordResetToken
	{
		[Key]
		public string UserToken { get; set; }
		public string IdentityToken { get; set; }
	}
}
