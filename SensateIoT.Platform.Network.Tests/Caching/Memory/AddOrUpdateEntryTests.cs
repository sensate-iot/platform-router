/*
 * Unit test the ability to add or update an entry.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Memory;

namespace SensateIoT.Platform.Network.Tests.Caching.Memory
{
	[TestClass]
	public class AddOrUpdateEntryTests
	{
		[TestMethod]
		public void CanAddAnEntry()
		{
			var cache = new MemoryCache<string, string>();

			cache.AddOrUpdate("abc", "def");
			Assert.AreEqual(1, cache.Count);
		}

		[TestMethod]
		public void CanUpdateSize()
		{
			var cache = new MemoryCache<string, string>(100);
			var opts = new CacheEntryOptions { Size = 10 };

			cache.AddOrUpdate("abc", "def", opts);
			Assert.AreEqual(1, cache.Count);
			Assert.AreEqual(10, cache.Size);
		}

		[TestMethod]
		public void WillThrowOnOverflow()
		{
			var cache = new MemoryCache<string, string>(10);
			var opts = new CacheEntryOptions { Size = 15 };

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => cache.AddOrUpdate("abc", "def", opts));
			Assert.AreEqual(0, cache.Count);
			Assert.AreEqual(0, cache.Size);
		}

		[TestMethod]
		public void CannotAddNullKey()
		{
			var cache = new MemoryCache<string, string>();
			Assert.ThrowsException<ArgumentNullException>(() => cache.AddOrUpdate(null, "bal"));
		}

		[TestMethod]
		public void CanAddNullValue()
		{
			var cache = new MemoryCache<string, string>();

			cache.AddOrUpdate("abc", null);
			Assert.AreEqual(1, cache.Count);
			Assert.AreEqual(0, cache.Size);
		}

		[TestMethod]
		public void MustGiveSizeOption()
		{
			var cache = new MemoryCache<int, string>(10);
			var options = new CacheEntryOptions();

			Assert.ThrowsException<ArgumentException>(() => cache.AddOrUpdate(10, "hello"));
			Assert.ThrowsException<ArgumentException>(() => cache.AddOrUpdate(10, "hello", options));
		}

		[TestMethod]
		public void CanReplaceADuplicate()
		{
			var cache = new MemoryCache<string, string>();

			cache.AddOrUpdate("abc", "def");
			cache.AddOrUpdate("abc", "deef");

			Assert.AreEqual(1, cache.Count);
			Assert.AreEqual(0, cache.Size);

			var result = cache.TryGetValue("abc", out var value);

			Assert.IsTrue(result);
			Assert.AreEqual("deef", value);
		}
	}
}
