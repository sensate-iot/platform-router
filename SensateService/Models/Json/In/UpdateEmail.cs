/*
 * Email change (JSON) viewmodel.
 */

namespace SensateService.Models.Json.In
{
	public class UpdateEmail
	{
		public string Email { get; set; }
		public string NewEmail { get; set; }
		public string Token { get; set; }
	}
}
