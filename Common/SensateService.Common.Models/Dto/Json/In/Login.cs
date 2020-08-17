/*
 * Json model to login.
 *
 * @author Michel Megens
 * @email   michel.megens@sonatolabs.com
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Common.Data.Dto.Json.In
{
	public class Login
	{
		[Required]
		public string Email { get; set; }
		[Required]
		public string Password { get; set; }
	}
}
