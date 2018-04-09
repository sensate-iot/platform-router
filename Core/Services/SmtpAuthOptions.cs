/*
 * SMTP authentication configuration options.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

namespace SensateService.Services
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
