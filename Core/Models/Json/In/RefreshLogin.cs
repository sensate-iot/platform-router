/*
 * Json model to refresh a JWT token.
 *
 * @author Michel Megens
 * @email   dev@bietje.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Models.Json.In
{
	public class RefreshLogin
	{
		[Required]
		public string Email { get; set; }
		[Required]
		public string RefreshToken { get; set; }
	}
}
