/*
 * Sensate cache configuration wrapper.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

namespace SensateService.Config
{
	public class CacheConfig
	{
		public bool Enabled { get; set; }
		public string Type { get; set; }
		public int Workers { get; set; }
	}
}
