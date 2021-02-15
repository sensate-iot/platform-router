/*
 * Trigger timeout configuration.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Platform.Network.TriggerService.Config
{
	public class TimeoutConfig
	{
		public int SmsTimeout { get; set; }
		public int MailTimeout { get; set; }
		public int HttpTimeout { get; set; }
		public int ActuatorTimeout { get; set; }
	}
}
