/*
 * Sensate mail configuration wrapper.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateIoT.API.Common.Config.Config
{
	public class MailConfig
	{
		public string Provider { get; set; }
		public string From { get; set; }
		public string FromName { get; set; }
		public SmtpConfig Smtp { get; set; }
		public SendGridConfig SendGrid { get; set; }
	}

	public class SmtpConfig
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Host { get; set; }
		public bool Ssl { get; set; }
		public short Port { get; set; }
	}

	public class SendGridConfig
	{
		public string Username { get; set; }
		public string Key { get; set; }
	}
}