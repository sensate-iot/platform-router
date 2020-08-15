/*
 * MQTT service init.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

namespace SensateService.Services.Settings
{
	public class CommandPublisherOptions
	{
		public string CommandsTopic { get; set; }
		public string Host { get; set; }
		public int Port { get; set; }
		public bool Ssl { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
	}
}