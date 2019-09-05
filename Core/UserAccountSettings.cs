/*
 * RESTful account controller.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

namespace SensateService
{
	public class UserAccountSettings
	{
		public string JwtKey { get; set; }
		public string JwtIssuer { get; set; }
		public int JwtExpireMinutes { get; set; }
		public int JwtRefreshExpireMinutes { get; set; }
		public string PublicUrl { get; set; }
		public string Scheme { get; set; }
	}
}
