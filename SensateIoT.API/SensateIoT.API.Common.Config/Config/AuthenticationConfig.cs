/*
 * Authentication configuration binding class.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateIoT.API.Common.Config.Config
{
	public class AuthenticationConfig
	{
		public int JwtRefreshExpireMinutes { get; set; }
		public int JwtExpireMinutes { get; set; }
		public string JwtKey { get; set; }
		public string JwtIssuer { get; set; }
		public string PublicUrl { get; set; }
		public string Scheme { get; set; }
		public bool PrimaryAuthHost { get; set; }
	}
}