/*
 * Unit tests for the bracket operator of MemoryCache<K,V>.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SensateIoT.Platform.Network.Common.Caching.Memory;

namespace SensateIoT.Platform.Network.Tests.Caching.Memory
{
	[TestClass]
	public class BracketOperatorTests
	{
		[TestMethod]
		public void CanSetAValue()
		{
			var cache = new MemoryCache<string, string>();

			cache["abc"] = "def";
			Assert.IsTrue(cache.TryGetValue("abc", out var result));
			Assert.AreEqual("def", result);
		}

		[TestMethod]
		public void CanUseObjectInitializer()
		{
			var cache = new MemoryCache<string, string> { ["abc"] = "def" };

			Assert.IsTrue(cache.TryGetValue("abc", out var result));
			Assert.AreEqual("def", result);
		}

		[TestMethod]
		public void CanGetAValue()
		{
			var cache = new MemoryCache<string, string>();

			cache.Add("abc", "def");
			Assert.AreEqual("def", cache["abc"]);
		}

		[TestMethod]
		public void CannotGetUnknownKey()
		{
			var cache = new MemoryCache<string, string>();
			Assert.ThrowsException<KeyNotFoundException>(() => cache["abc"]);
		}

		[TestMethod]
		public void CanSetAIsNullValue()
		{
			var cache = new MemoryCache<string, string>();

			cache["abc"] = null;
			Assert.IsNull(cache["abc"]);
			Assert.AreEqual(1, cache.Count);
		}

		[TestMethod]
		public void CannotSetAIsNullKey()
		{
			var cache = new MemoryCache<string, string>();

			Assert.ThrowsException<ArgumentNullException>(() => cache[null] = "abc");
			Assert.AreEqual(0, cache.Count);
		}

		[TestMethod]
		public void CanUpdateAValue()
		{
			var cache = new MemoryCache<string, string>();

			cache["abc"] = "def";
			cache["abc"] = "abc";

			Assert.AreEqual("abc", cache["abc"]);
		}

		[TestMethod]
		public void CannotGetTimeoutValue()
		{
			var cache = new MemoryCache<string, string>(MemoryCache<string, string>.DefaultCapacity, TimeSpan.FromMilliseconds(50));

			cache["abc"] = "def";
			Assert.AreEqual("def", cache["abc"]);
			Thread.Sleep(55);
			Assert.ThrowsException<KeyNotFoundException>(() => cache["abc"]);
		}
	}
}
