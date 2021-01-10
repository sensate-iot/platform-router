/*
 * Json model to refresh a JWT token.
 *
 * @author Michel Megens
 * @email   michel.megens@sonatolabs.com
 */

using System.ComponentModel.DataAnnotations;

namespace SensateIoT.API.Common.Data.Dto.Json.In
{
	public class RefreshLogin
	{
		[Required]
		public string Email { get; set; }
		[Required]
		public string RefreshToken { get; set; }
	}
}
