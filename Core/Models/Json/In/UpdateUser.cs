/*
 * Update user view model.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateService.Models.Json.In
{
	public class UpdateUser
	{
		public string FirstName { get; set;}
		public string LastName { get; set; }
		public string Password { get; set; }
		public string CurrentPassword { get; set; }
	}
}
