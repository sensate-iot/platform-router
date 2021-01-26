/*
 * Test the ability to clean-up timed-out entry's.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Memory;

namespace SensateIoT.Platform.Network.Tests.Caching.Memory
{
	[TestClass]
	public class TimeoutTests
	{
		[TestMethod]
		public async Task CanAutomaticallyRemoveTimedoutEntriesAsync()
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
			await cache.RemoveScheduledEntriesAsync().ConfigureAwait(false);

			Assert.ThrowsException<KeyNotFoundException>(() => cache[5]);
			Assert.ThrowsException<KeyNotFoundException>(() => cache[6]);
			Assert.AreEqual(20, cache[10]);
			Assert.AreEqual(21, cache[11]);
		}

		[TestMethod]
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

			Assert.ThrowsException<KeyNotFoundException>(() => cache[5]);
			Assert.ThrowsException<KeyNotFoundException>(() => cache[6]);
			Assert.AreEqual(20, cache[10]);
			Assert.AreEqual(21, cache[11]);
		}

	}
}
