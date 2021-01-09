/*
 * Unit tests for MemoryCache<K,V>.TryGetValue().
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SensateIoT.Platform.Network.Common.Caching.Memory;

namespace SensateIoT.Platform.Network.Tests.Caching.Memory
{
	[TestClass]
	public class TryGetValueTests
	{
		[TestMethod]
		public void CanGetAnExistingValue()
		{
			var cache = new MemoryCache<int, int>();

			cache.Add(1, 2);
			Assert.IsTrue(cache.TryGetValue(1, out var result));
			Assert.AreEqual(2, result);
		}

		[TestMethod]
		public void CannotGetNonExistingValue()
		{
			var cache = new MemoryCache<int, int>();

			Assert.IsFalse(cache.TryGetValue(1, out var result));
			Assert.AreEqual(default, result);
		}

		[TestMethod]
		public void CannotGetIsNullKey()
		{
			var cache = new MemoryCache<string, int>();
			Assert.ThrowsException<ArgumentNullException>(() => cache.TryGetValue(null, out _));
		}

		[TestMethod]
		public void CanGetIsNullValue()
		{
			var cache = new MemoryCache<string, string>();

			cache.Add("abc", null);
			Assert.IsTrue(cache.TryGetValue("abc", out var result));
			Assert.IsNull(result);
		}

		[TestMethod]
		public void CannotGetDeletedKey()
		{
			var cache = new MemoryCache<int, int>();

			cache.Add(1, 2);
			cache.Remove(1);
			Assert.IsFalse(cache.TryGetValue(1, out _));
		}

		[TestMethod]
		public void CannotGetExpiredKey()
		{
			var cache = new MemoryCache<int, int>(MemoryCache<int, int>.DefaultCapacity, TimeSpan.FromMilliseconds(50));

			cache.Add(1, 2);
			Thread.Sleep(51);
			Assert.IsFalse(cache.TryGetValue(1, out var result));
			Assert.AreEqual(default, result);
		}

		[TestMethod]
		public void CanGetKeyWithExpiry()
		{
			var cache = new MemoryCache<int, int>(MemoryCache<int, int>.DefaultCapacity, TimeSpan.FromMilliseconds(50));

			cache.Add(1, 2);
			Assert.IsTrue(cache.TryGetValue(1, out var result));
			Assert.AreEqual(2, result);
		}
	}
}
