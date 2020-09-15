/*
 * Unit tests for MemoryCache<K,V>.TryGetValue().
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;
using SensateService.Common.Caching.Memory;
using Xunit;

namespace SensateService.Common.Caching.Tests.Memory
{
	public class TryGetValueTests
	{
		[Fact]
		public void CanGetAnExistingValue()
		{
			var cache = new MemoryCache<int, int>();

			cache.Add(1, 2);
			Assert.True(cache.TryGetValue(1, out var result));
			Assert.Equal(2, result);
		}

		[Fact]
		public void CannotGetNonExistingValue()
		{
			var cache = new MemoryCache<int, int>();

			Assert.False(cache.TryGetValue(1, out var result));
			Assert.Equal(default, result);
		}

		[Fact]
		public void CannotGetNullKey()
		{
			var cache = new MemoryCache<string, int>();
			Assert.Throws<ArgumentNullException>(() => cache.TryGetValue(null, out _));
		}

		[Fact]
		public void CanGetNullValue()
		{
			var cache = new MemoryCache<string, string>();

			cache.Add("abc", null);
			Assert.True(cache.TryGetValue("abc", out var result));
			Assert.Null(result);
		}

		[Fact]
		public void CannotGetDeletedKey()
		{
			var cache = new MemoryCache<int, int>();

			cache.Add(1, 2);
			cache.Remove(1);
			Assert.False(cache.TryGetValue(1, out _));
		}

		[Fact]
		public void CannotGetExpiredKey()
		{
			var cache = new MemoryCache<int, int>(MemoryCache<int, int>.DefaultCapacity, 50);

			cache.Add(1, 2);
			Thread.Sleep(51);
			Assert.False(cache.TryGetValue(1, out var result));
			Assert.Equal(default, result);
		}

		[Fact]
		public void CanGetKeyWithExpiry()
		{
			var cache = new MemoryCache<int, int>(MemoryCache<int, int>.DefaultCapacity, 50);

			cache.Add(1, 2);
			Assert.True(cache.TryGetValue(1, out var result));
			Assert.Equal(2, result);
		}
	}
}
