/*
 * Unit tests for the bracket operator of MemoryCache<K,V>.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;
using SensateIoT.Common.Caching.Memory;
using Xunit;

namespace SensateIoT.Common.Caching.Tests.Memory
{
	public class BracketOperatorTests
	{
		[Fact]
		public void CanSetAValue()
		{
			var cache = new MemoryCache<string, string>();

			cache["abc"] = "def";
			Assert.True(cache.TryGetValue("abc", out var result));
			Assert.Equal("def", result);
		}

		[Fact]
		public void CanUseObjectInitializer()
		{
			var cache = new MemoryCache<string, string> { ["abc"] = "def" };

			Assert.True(cache.TryGetValue("abc", out var result));
			Assert.Equal("def", result);
		}

		[Fact]
		public void CanGetAValue()
		{
			var cache = new MemoryCache<string, string>();

			cache.Add("abc", "def");
			Assert.Equal("def", cache["abc"]);
		}

		[Fact]
		public void CannotGetUnknownKey()
		{
			var cache = new MemoryCache<string, string>();
			Assert.Throws<ArgumentOutOfRangeException>(() => cache["abc"]);
		}

		[Fact]
		public void CanSetANullValue()
		{
			var cache = new MemoryCache<string, string>();

			cache["abc"] = null;
			Assert.Null(cache["abc"]);
			Assert.Equal(1, cache.Count);
		}

		[Fact]
		public void CannotSetANullKey()
		{
			var cache = new MemoryCache<string, string>();

			Assert.Throws<ArgumentNullException>(() => cache[null] = "abc");
			Assert.Equal(0, cache.Count);
		}

		[Fact]
		public void CanUpdateAValue()
		{
			var cache = new MemoryCache<string, string>();

			cache["abc"] = "def";
			cache["abc"] = "abc";

			Assert.Equal("abc", cache["abc"]);
		}

		[Fact]
		public void CannotGetTimeoutValue()
		{
			var cache = new MemoryCache<string, string>(MemoryCache<string, string>.DefaultCapacity, 50);

			cache["abc"] = "def";
			Assert.Equal("def", cache["abc"]);
			Thread.Sleep(55);
			Assert.Throws<ArgumentOutOfRangeException>(() => cache["abc"]);
		}
	}
}
