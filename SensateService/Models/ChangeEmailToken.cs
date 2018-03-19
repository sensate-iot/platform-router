/*
 * Change email token bridge.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Models
{
	public class ChangeEmailToken
	{
		[Key]
		public string IdentityToken { get; set; }
		public string UserToken { get; set; }
		public string Email {get; set;}
	}
}
