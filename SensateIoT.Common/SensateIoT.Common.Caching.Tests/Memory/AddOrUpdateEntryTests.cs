/*
 * Unit test the ability to add or update an entry.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using SensateIoT.Common.Caching.Abstract;
using SensateIoT.Common.Caching.Memory;
using Xunit;

namespace SensateIoT.Common.Caching.Tests.Memory
{
	public class AddOrUpdateEntryTests
	{
		[Fact]
		public void CanAddAnEntry()
		{
			var cache = new MemoryCache<string, string>();

			cache.AddOrUpdate("abc", "def");
			Assert.Equal(1, cache.Count);
		}

		[Fact]
		public void CanUpdateSize()
		{
			var cache = new MemoryCache<string, string>(100);
			var opts = new CacheEntryOptions { Size = 10 };

			cache.AddOrUpdate("abc", "def", opts);
			Assert.Equal(1, cache.Count);
			Assert.Equal(10, cache.Size);
		}

		[Fact]
		public void WillThrowOnOverflow()
		{
			var cache = new MemoryCache<string, string>(10);
			var opts = new CacheEntryOptions { Size = 15 };

			Assert.Throws<ArgumentOutOfRangeException>(() => cache.AddOrUpdate("abc", "def", opts));
			Assert.Equal(0, cache.Count);
			Assert.Equal(0, cache.Size);
		}

		[Fact]
		public void CannotAddNullKey()
		{
			var cache = new MemoryCache<string, string>();
			Assert.Throws<ArgumentNullException>(() => cache.AddOrUpdate(null, "bal"));
		}

		[Fact]
		public void CanAddNullValue()
		{
			var cache = new MemoryCache<string, string>();

			cache.AddOrUpdate("abc", null);
			Assert.Equal(1, cache.Count);
			Assert.Equal(0, cache.Size);
		}

		[Fact]
		public void MustGiveSizeOption()
		{
			var cache = new MemoryCache<int, string>(10);
			var options = new CacheEntryOptions();

			Assert.Throws<ArgumentException>(() => cache.AddOrUpdate(10, "hello"));
			Assert.Throws<ArgumentException>(() => cache.AddOrUpdate(10, "hello", options));
		}

		[Fact]
		public void CanReplaceADuplicate()
		{
			var cache = new MemoryCache<string, string>();

			cache.AddOrUpdate("abc", "def");
			cache.AddOrUpdate("abc", "deef");

			Assert.Equal(1, cache.Count);
			Assert.Equal(0, cache.Size);

			var result = cache.TryGetValue("abc", out var value);

			Assert.True(result);
			Assert.Equal("deef", value);
		}
	}
}
