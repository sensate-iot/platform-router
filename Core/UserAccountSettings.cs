/*
 * RESTful account controller.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

namespace SensateService
{
	public class UserAccountSettings
	{
		public string JwtKey { get; set; }
		public string JwtIssuer { get; set; }
		public int JwtExpireMinutes { get; set; }
		public int JwtRefreshExpireMinutes { get; set; }
		public string ConfirmForward { get; set; }
	}
}
