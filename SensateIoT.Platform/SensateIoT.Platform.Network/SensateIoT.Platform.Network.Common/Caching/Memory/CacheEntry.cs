/*
 * Cache entry for in-memory caches.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using SensateIoT.Platform.Network.Common.Caching.Internal;

namespace SensateIoT.Platform.Network.Common.Caching.Memory
{
	public class CacheEntry<TValue>
	{
		public TValue Value { get; set; }
		public TimeSpan Timeout { get; set; }
		public DateTimeOffset Timestamp { get; set; }
		public EntryState State { get; set; }
		public long Size { get; set; }
	}
}
