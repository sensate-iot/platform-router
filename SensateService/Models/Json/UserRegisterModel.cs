/*
 * Json model to register a new user.
 *
 * @author Michel Megens
 * @email   dev@bietje.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Models.Json
{
	public class RegisterModel
	{
		[Required]
		public string Email { get; set; }
		[Required]
		[StringLength(64, ErrorMessage = "Password must be between 6 and 64 characters!", MinimumLength = 6)]
		public string Password { get; set; }
		[Required]
		public string FirstName {get;set;}
		[Required]
		public string LastName {get;set;}
		public string PhoneNumber {get;set;}
	}
}
