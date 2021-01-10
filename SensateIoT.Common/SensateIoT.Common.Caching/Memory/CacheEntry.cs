/*
 * Cache entry for in-memory caches.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using SensateIoT.Common.Caching.Internal;

namespace SensateIoT.Common.Caching.Memory
{
	public class CacheEntry<TValue>
	{
		public TValue Value { get; set; }
		public int Timeout { get; set; } // Timeout in milliseconds.
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset LastSeen { get; set; }
		public EntryState State { get; set; }
		public long Size { get; set; }
	}
}
