/*
 * MQTT background service
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

namespace SensateIoT.Platform.Ingress.Common.Options
{
	public class MqttServiceOptions
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public bool Ssl { get; set; }
		public string Id { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string TopicShare { get; set; }
	}
}
