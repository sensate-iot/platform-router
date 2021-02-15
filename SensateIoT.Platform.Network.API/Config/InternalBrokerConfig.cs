/*
 * Internal broker configuration.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Platform.Network.API.Config
{
	public class InternalBrokerConfig
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Host { get; set; }
		public bool Ssl { get; set; }
		public short Port { get; set; }
		public string CommandTopic { get; set; }
	}
}
