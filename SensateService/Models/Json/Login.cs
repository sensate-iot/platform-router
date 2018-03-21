/*
 * Json model to login.
 *
 * @author Michel Megens
 * @email   dev@bietje.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Models.Json
{
	public class Login
	{
		[Required]
		public string Email { get; set; }
		[Required]
		public string Password { get; set; }
	}
}
