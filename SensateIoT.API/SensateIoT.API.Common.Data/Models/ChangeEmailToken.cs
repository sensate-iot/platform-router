/*
 * Change email token bridge.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SensateIoT.API.Common.Data.Models
{
	[Table("EmailTokens")]
	public class ChangeEmailToken
	{
		[Key]
		public string IdentityToken { get; set; }
		public string UserToken { get; set; }
		public string Email { get; set; }
	}
}
