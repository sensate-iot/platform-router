/*
 * Sensate mail configuration wrapper.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateService.Config
{
	public class MqttConfig
	{
		public PublicMqttConfig PublicBroker { get; set; }
		public PrivateMqttConfig InternalBroker { get; set; }
	}

	public class PublicMqttConfig
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Host { get; set; }
		public bool Ssl { get; set; }
		public short Port { get; set; }
		public string ShareTopic { get; set; }
		public string BulkShareTopic { get; set; }
		public string RealTimeShareTopic { get; set; }
		public string MessageTopic { get; set; }
		public string ActuatorTopic { get; set; }
	}

	public class PrivateMqttConfig
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Host { get; set; }
		public bool Ssl { get; set; }
		public short Port { get; set; }
		public string InternalBulkMeasurementTopic { get; set; }
		public string InternalBulkMessageTopic { get; set; }
		public string AuthorizedBulkMessageTopic { get; set; }
		public string AuthorizedBulkMeasurementTopic { get; set; }
		public string InternalBlobTopic { get; set; }
	}
}