/*
 * Update user view model.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Models.Json.In
{
	public class UpdateUser
	{
		public string FirstName { get; set;}
		public string LastName { get; set; }
		public string Password { get; set; }
		public string CurrentPassword { get; set; }
		public string PhoneNumber { get; set; }
	}
}
