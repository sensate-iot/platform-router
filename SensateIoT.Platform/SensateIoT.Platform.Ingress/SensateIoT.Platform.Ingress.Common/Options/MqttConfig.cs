/*
 * Sensate mail configuration wrapper.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateIoT.Platform.Ingress.Common.Options
{
	public class MqttConfig
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Host { get; set; }
		public bool Ssl { get; set; }
		public short Port { get; set; }
		public string MeasurementTopic { get; set; }
		public string MessageTopic { get; set; }
	}
}
