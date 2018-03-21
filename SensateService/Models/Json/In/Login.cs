/*
 * Json model to login.
 *
 * @author Michel Megens
 * @email   dev@bietje.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Models.Json.In
{
	public class Login
	{
		[Required]
		public string Email { get; set; }
		public string Password { get; set; }
		public string RefreshToken { get; set; }
	}
}
