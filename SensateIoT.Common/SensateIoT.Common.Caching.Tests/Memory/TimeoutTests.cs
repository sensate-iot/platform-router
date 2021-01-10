/*
 * Test the ability to clean-up timed-out entry's.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;
using SensateIoT.Common.Caching.Abstract;
using SensateIoT.Common.Caching.Memory;
using Xunit;

namespace SensateIoT.Common.Caching.Tests.Memory
{
	public class TimeoutTests
	{
		[Fact]
		public void CanAutomaticallyRemoveTimedoutEntries()
		{
			var cache = new MemoryCache<int, int>();

			var opts = new CacheEntryOptions {
				Timeout = TimeSpan.FromMilliseconds(100)
			};

			cache.Add(5, 10, opts);
			cache.Add(6, 11, opts);

			opts.Timeout = TimeSpan.FromMilliseconds(5000);

			cache.Add(10, 20, opts);
			cache.Add(11, 21, opts);

			Thread.Sleep(150);

			cache.ScanForExpiredItems();
			cache.RemoveScheduledEntries();

			Assert.Throws<ArgumentOutOfRangeException>(() => cache[5]);
			Assert.Throws<ArgumentOutOfRangeException>(() => cache[6]);
			Assert.Equal(20, cache[10]);
			Assert.Equal(21, cache[11]);
		}
	}
}
