/*
 * Add entry unit tests.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Memory;

namespace SensateIoT.Platform.Network.Tests.Caching.Memory
{
	[TestClass]
	public class AddEntryTests
	{
		[TestMethod]
		public void CanAddAnEntry()
		{
			var cache = new MemoryCache<string, string>();

			cache.Add("abc", "def");
			Assert.AreEqual(1, cache.Count);
		}

		[TestMethod]
		public void CanUpdateSize()
		{
			var cache = new MemoryCache<string, string>(100);
			var opts = new CacheEntryOptions { Size = 10 };

			cache.Add("abc", "def", opts);
			Assert.AreEqual(1, cache.Count);
			Assert.AreEqual(10, cache.Size);
		}

		[TestMethod]
		public void WillThrowOnOverflow()
		{
			var cache = new MemoryCache<string, string>(10);
			var opts = new CacheEntryOptions { Size = 15 };

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => cache.Add("abc", "def", opts));
			Assert.AreEqual(0, cache.Count);
			Assert.AreEqual(0, cache.Size);
		}

		[TestMethod]
		public void CannotAddNullKey()
		{
			var cache = new MemoryCache<string, string>();
			Assert.ThrowsException<ArgumentNullException>(() => cache.Add(null, "bal"));
		}

		[TestMethod]
		public void CanAddNullValue()
		{
			var cache = new MemoryCache<string, string>();

			cache.Add("abc", null);
			Assert.AreEqual(1, cache.Count);
			Assert.AreEqual(0, cache.Size);
		}

		[TestMethod]
		public void MustGiveSizeOption()
		{
			var cache = new MemoryCache<int, string>(10);
			var options = new CacheEntryOptions();

			Assert.ThrowsException<ArgumentException>(() => cache.Add(10, "hello"));
			Assert.ThrowsException<ArgumentException>(() => cache.Add(10, "hello", options));
		}

		[TestMethod]
		public void CannotReplaceDuplicate()
		{
			var cache = new MemoryCache<string, string>();

			cache.Add("abc", "def");
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => cache.Add("abc", "dab"));
		}

		[TestMethod]
		public void CanReplaceTimedOutEntry()
		{
			var cache = new MemoryCache<string, string>(MemoryCache<string, string>.DefaultCapacity, TimeSpan.FromMilliseconds(100));

			cache.Add("abc", "def");
			Assert.AreEqual(1, cache.Count);
			Thread.Sleep(150);
			cache.Add("abc", "gfh");
			Assert.AreEqual(1, cache.Count);
		}

		[TestMethod]
		public void EntryCanTimeout()
		{
			var cache = new MemoryCache<string, string>(100, TimeSpan.FromMilliseconds(100));

			cache.Add("abc", "def", new CacheEntryOptions {
				Size = 10
			});

			Assert.AreEqual(1, cache.Count);
			Thread.Sleep(150);
			Assert.IsFalse(cache.TryGetValue("abc", out _));
		}
	}
}
