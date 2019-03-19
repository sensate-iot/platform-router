/*
 * Sensate mail configuration wrapper.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
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
	}

	public class PrivateMqttConfig
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Host { get; set; }
		public bool Ssl { get; set; }
		public short Port { get; set; }
		public string InternalMeasurementTopic { get; set; }
		public string InternalBulkMeasurementTopic { get; set; }
	}
}