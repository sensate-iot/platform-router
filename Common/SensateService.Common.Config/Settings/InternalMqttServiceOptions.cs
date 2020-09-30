/*
 * Options for the internal MQTT service.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateService.Common.Config.Settings
{
	public class InternalMqttServiceOptions
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public bool Ssl { get; set; }
		public string Id { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string AuthorizedBulkMeasurementTopic { get; set; }
		public string AuthorizedBulkMessageTopic { get; set; }
		public string InternalBulkMeasurementTopic { get; set; }
		public string InternalBulkMessageTopic { get; set; }
		public string InternalBlobTopic { get; set; }
		public string TopicShare { get; set; }
	}
}
