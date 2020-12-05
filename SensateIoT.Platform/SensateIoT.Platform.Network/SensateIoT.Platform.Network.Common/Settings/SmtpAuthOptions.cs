/*
 * SMTP authentication configuration options.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateIoT.Platform.Network.Common.Settings
{
	public sealed class SmtpAuthOptions
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string From { get; set; }
		public string FromName { get; set; }
		public string Host { get; set; }
		public short Port { get; set; }
		public bool Ssl { get; set; }
	}
}
