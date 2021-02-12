/*
 * SendGrid authentication configuration options.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateIoT.Platform.Network.Adapters.Mail
{
	public sealed class SendGridAuthOptions
	{
		public string Key { get; set; }
		public string Username { get; set; }
		public string From { get; set; }
		public string FromName { get; set; }
	}
}