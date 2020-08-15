/*
 * SendGrid authentication configuration options.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateService.Services.Settings
{
	public sealed class SendGridAuthOptions : MessageSenderAuthOptions
	{
		public string Key { get; set; }
		public string Username { get; set; }
	}
}