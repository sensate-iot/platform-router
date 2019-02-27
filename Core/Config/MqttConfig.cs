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
		public string Username { get; set; }
		public string Password { get; set; }
		public string Host { get; set; }
		public bool Ssl { get; set; }
		public short Port { get; set; }
		public string ShareTopic { get; set; }
		public string InternalMeasurementTopic { get; set; }
		public string InternalBulkMeasurementTopic { get; set; }
	}
}