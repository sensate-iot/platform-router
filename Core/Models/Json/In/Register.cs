/*
 * Json model to register a new user.
 *
 * @author Michel Megens
 * @email   dev@bietje.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Models.Json.In
{
	public class Register
	{
		[Required]
		public string Email { get; set; }
		[Required]
		public string Password { get; set; }
		[Required]
		public string FirstName {get;set;}
		[Required]
		public string LastName {get;set;}
		public string PhoneNumber {get;set;}
		public string ForwardTo { get; set; }
	}
}
