/*
 * MQTT configuration.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Platform.Network.Router.Config
{
	public class MqttConfig
	{
		public PublicBrokerConfig PublicBroker { get; set; }
		public InternalBrokerConfig InternalBroker { get; set; }
	}
}