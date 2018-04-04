/*
 * Enumeration of cache timeouts.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

namespace SensateService.Infrastructure
{
	public enum CacheTimeout
	{
		Timeout = 10,
		TimeoutShort = 1,
		TimeoutMedium = 4,
		TimeoutLong = 15
	}
}
