/*
 * Add entry unit tests.
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
	public class AddEntryTests
	{
		[Fact]
		public void CanAddAnEntry()
		{
			var cache = new MemoryCache<string, string>();

			cache.Add("abc", "def");
			Assert.Equal(1, cache.Count);
		}

		[Fact]
		public void CanUpdateSize()
		{
			var cache = new MemoryCache<string, string>(100);
			var opts = new CacheEntryOptions { Size = 10 };

			cache.Add("abc", "def", opts);
			Assert.Equal(1, cache.Count);
			Assert.Equal(10, cache.Size);
		}

		[Fact]
		public void WillThrowOnOverflow()
		{
			var cache = new MemoryCache<string, string>(10);
			var opts = new CacheEntryOptions { Size = 15 };

			Assert.Throws<ArgumentOutOfRangeException>(() => cache.Add("abc", "def", opts));
			Assert.Equal(0, cache.Count);
			Assert.Equal(0, cache.Size);
		}

		[Fact]
		public void CannotAddNullKey()
		{
			var cache = new MemoryCache<string, string>();
			Assert.Throws<ArgumentNullException>(() => cache.Add(null, "bal"));
		}

		[Fact]
		public void CanAddNullValue()
		{
			var cache = new MemoryCache<string, string>();

			cache.Add("abc", null);
			Assert.Equal(1, cache.Count);
			Assert.Equal(0, cache.Size);
		}

		[Fact]
		public void MustGiveSizeOption()
		{
			var cache = new MemoryCache<int, string>(10);
			var options = new CacheEntryOptions();

			Assert.Throws<ArgumentException>(() => cache.Add(10, "hello"));
			Assert.Throws<ArgumentException>(() => cache.Add(10, "hello", options));
		}

		[Fact]
		public void CannotReplaceDuplicate()
		{
			var cache = new MemoryCache<string, string>();

			cache.Add("abc", "def");
			Assert.Throws<ArgumentOutOfRangeException>(() => cache.Add("abc", "def"));
		}

		[Fact]
		public void CanReplaceTimedOutEntry()
		{
			var cache = new MemoryCache<string, string>(MemoryCache<string, string>.DefaultCapacity, 100);

			cache.Add("abc", "def");
			Assert.Equal(1, cache.Count);
			Thread.Sleep(150);
			cache.Add("abc", "gfh");
			Assert.Equal(1, cache.Count);
		}

		[Fact]
		public void EntryCanTimeout()
		{
			var cache = new MemoryCache<string, string>(100, 100);

			cache.Add("abc", "def", new CacheEntryOptions {
				Size = 10
			});

			Assert.Equal(1, cache.Count);
			Thread.Sleep(150);
			Assert.False(cache.TryGetValue("abc", out _));
		}
	}
}
