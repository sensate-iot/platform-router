/*
 * Sensate mail configuration wrapper.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateService.Common.Config.Config
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
		public string MeasurementTopic { get; set; }
		public string BulkMeasurementTopic { get; set; }
		public string MessageTopic { get; set; }
		public string BulkMessageTopic { get; set; }
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
		public string InternalCommandsTopic { get; set; }
	}
}