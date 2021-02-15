/*
 * Router client settings.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Platform.Network.TriggerService.Settings
{
	public class RouterSettings
	{
		public string Host { get; set; }
		public ushort Port { get; set; }
		public bool Secure { get; set; }
	}
}
