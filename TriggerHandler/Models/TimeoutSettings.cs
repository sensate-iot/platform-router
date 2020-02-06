/*
 * Text messaging settings.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateService.TriggerHandler.Models
{
	public class TimeoutSettings
	{
		public int MessageTimeout { get; set; }
		public int MailTimeout { get; set; }
		public int MqttTimeout { get; set; }
		public int HttpTimeout { get; set; }
	}
}