/*
 * MQTT configuration.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Platform.Router.Service.Config
{
	public class MqttConfig
	{
		public PublicBrokerConfig PublicBroker { get; set; }
		public InternalBrokerConfig InternalBroker { get; set; }
	}
}