/*
 * Authentication configuration binding class.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

namespace SensateService.Config
{
	public class AuthenticationConfig
	{
		public string ConfirmForward { get; set; }
        public int JwtRefreshExpireMinutes { get; set; }
        public int JwtExpireMinutes { get; set; }
        public string JwtKey { get; set; }
        public string JwtIssuer { get; set; }
		public string ResetForward { get; set; }
		public string PublicUrl { get; set; }
	}
}