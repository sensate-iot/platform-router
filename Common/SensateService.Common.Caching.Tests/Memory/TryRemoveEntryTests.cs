/*
 * Unit tests for MemoryCache.TryRemove().
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;
using Xunit;

using SensateService.Common.Caching.Memory;

namespace SensateService.Common.Caching.Tests.Memory
{
	public class TryRemoveEntryTests
	{
		[Fact]
		public void RemoveDoesNotInstantlyDelete()
		{
			var cache = new MemoryCache<int, int>();

			cache.Add(1, 2);
			cache.TryRemove(1);

			Assert.Equal(1, cache.Count);
		}

		[Fact]
		public void RemoveDoesDeleteAfterScan()
		{
			var cache = new MemoryCache<int, int>();

			cache.Add(1, 2);
			cache.TryRemove(1);
			cache.RemoveScheduledEntries();

			Assert.Equal(0, cache.Count);
		}

		[Fact]
		public void CannotRemoveNullKey()
		{
			var cache = new MemoryCache<string, int>();
			Assert.Throws<ArgumentNullException>(() => cache.Remove(null));
		}

		[Fact]
		public void CannotRemoveNonExistingKey()
		{
			var cache = new MemoryCache<int, int>();

			cache.Add(1, 2);
			Assert.False(cache.TryRemove(2));
		}

		[Fact]
		public void CannotRemovePreviouslyRemovedKey()
		{
			var cache = new MemoryCache<int, int>();

			cache.Add(1, 2);
			Assert.True(cache.TryRemove(1));
			Assert.False(cache.TryRemove(1));
		}


		[Fact]
		public void CanRemoveExpiredItem()
		{
			var cache = new MemoryCache<int, int>(MemoryCache<int, int>.DefaultCapacity, 100);

			cache.Add(1, 2);
			Thread.Sleep(150);
			Assert.True(cache.TryRemove(1));
			cache.RemoveScheduledEntries();

			Assert.Equal(0, cache.Count);
		}

		[Fact]
		public void DeletionScanRunsOncePerTimeUnit()
		{
			var cache = new MemoryCache<int, int>();

			cache.Add(1, 1);
			Assert.Equal(1, cache.Count);
			cache.Remove(1);

			Thread.Sleep(10);
			Assert.Equal(0, cache.Count);

			cache.Add(1, 1);
			Assert.Equal(1, cache.Count);
			cache.Remove(1);
			Thread.Sleep(10);
			Assert.Equal(1, cache.Count);
		}
	}
}
