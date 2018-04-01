/*
 * Forgot password JSON model.
 *
 * @author Michel Megens
 * @email   dev@bietje.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Models.Json.In
{
	public class ResetPassword
	{
		[Required]
		public string Email {get;set;}
		[Required]
		public string Password {get;set;}
		[Required]
		public string Token {get;set;}
	}
}
