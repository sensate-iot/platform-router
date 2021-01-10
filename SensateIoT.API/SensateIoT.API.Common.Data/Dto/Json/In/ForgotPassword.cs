/*
 * Forgot password JSON model.
 *
 * @author Michel Megens
 * @email   michel.megens@sonatolabs.com
 */

using System.ComponentModel.DataAnnotations;

namespace SensateIoT.API.Common.Data.Dto.Json.In
{
	public class ForgotPassword
	{
		[Required]
		public string Email { get; set; }
	}
}
