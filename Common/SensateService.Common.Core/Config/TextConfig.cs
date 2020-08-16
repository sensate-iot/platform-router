/*
 * Text messaging configuration.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateService.Config
{
	public class TextConfig
	{
		public string Provider { get; set; }
		public string AlphaCode { get; set; }
		public TwilioConfig Twilio { get; set; }
	}

	public class TwilioConfig
	{
		public string AccountSid { get; set; }
		public string AuthToken { get; set; }
		public string PhoneSid { get; set; }
	}
}