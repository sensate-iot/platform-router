/*
 * State enum to track the state of various types
 * of cache entry's.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Common.Caching.Internal
{
	public enum EntryState
	{
		None,
		Expired,
		ScheduledForRemoval,
		Removed
	}
}
