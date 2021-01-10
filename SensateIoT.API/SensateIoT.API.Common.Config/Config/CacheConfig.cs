/*
 * Sensate cache configuration wrapper.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateIoT.API.Common.Config.Config
{
	public class CacheConfig
	{
		public bool Enabled { get; set; }
		public string Type { get; set; }
		public int Interval { get; set; }
	}
}
