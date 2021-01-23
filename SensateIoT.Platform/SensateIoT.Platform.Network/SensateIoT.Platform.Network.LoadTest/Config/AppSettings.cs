/*
 * Load test application settings.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Platform.Network.LoadTest.Config
{
	public class AppSettings
	{
		public string SensorIdPath { get; set; }
		public string RouterHostname { get; set; }
		public ushort RouterPort { get; set; }
		public int BatchSize { get; set; }
	}
}
