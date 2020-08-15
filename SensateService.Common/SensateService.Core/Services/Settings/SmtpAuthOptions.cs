/*
 * SMTP authentication configuration options.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateService.Services.Settings
{
	public sealed class SmtpAuthOptions : MessageSenderAuthOptions
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Host { get; set; }
		public short Port { get; set; }
		public bool Ssl { get; set; }
	}
}
