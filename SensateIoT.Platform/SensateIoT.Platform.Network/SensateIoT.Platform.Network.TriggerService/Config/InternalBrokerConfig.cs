/*
 * MQTT configuration.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Platform.Network.TriggerService.Config
{
	public class InternalBrokerConfig
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Host { get; set; }
		public bool Ssl { get; set; }
		public short Port { get; set; }
		public string BulkMeasurementTopic { get; set; }
		public string BulkMessageTopic { get; set; }
		public string ActuatorTopicFormat { get; set; }
	}
}
