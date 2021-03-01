/*
 * Sensate mail configuration wrapper.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using JetBrains.Annotations;

namespace SensateIoT.Platform.Network.TriggerService.Config
{
	public class MailConfig
	{
		public string Provider { get; set; }
		public string From { get; set; }
		public string FromName { get; set; }
		public SmtpConfig Smtp { get; set; }
		public SendGridConfig SendGrid { get; set; }
	}

	[UsedImplicitly]
	public class SmtpConfig
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Host { get; set; }
		public bool Ssl { get; set; }
		public short Port { get; set; }
	}

	[UsedImplicitly]
	public class SendGridConfig
	{
		public string Username { get; set; }
		public string Key { get; set; }
	}
}
