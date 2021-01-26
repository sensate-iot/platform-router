/*
 * Add or update range unit tests.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading;
using SensateIoT.Common.Caching.Abstract;
using SensateIoT.Common.Caching.Memory;
using Xunit;

namespace SensateIoT.Common.Caching.Tests.Memory
{
	public class AddOrUpdateRangeTests
	{
		[Fact]
		public void CanAddMultipleKVPairs()
		{
			var cache = new MemoryCache<string, string>();
			var values = new List<Caching.Abstract.KeyValuePair<string, string>>();

			for(var idx = 0; idx < 100; idx++) {
				var kvp = new Caching.Abstract.KeyValuePair<string, string> {
					Key = $"key::{idx}",
					Value = $"value::{idx}"
				};

				values.Add(kvp);
			}

			cache.AddOrUpdate(values);
			Assert.Equal("value::50", cache["key::50"]);
		}

		[Fact]
		public void CanAddAndUpdate()
		{
			var cache = new MemoryCache<string, string>();
			var values = new List<Caching.Abstract.KeyValuePair<string, string>>();

			cache["key::10"] = "abc";
			cache["key::50"] = "def";
			cache["key::55"] = "duf";

			Assert.Equal("abc", cache["key::10"]);
			Assert.Equal("def", cache["key::50"]);
			Assert.Equal("duf", cache["key::55"]);

			for(var idx = 0; idx < 100; idx++) {
				var kvp = new Caching.Abstract.KeyValuePair<string, string> {
					Key = $"key::{idx}",
					Value = $"value::{idx}"
				};

				values.Add(kvp);
			}

			cache.AddOrUpdate(values);
			Assert.Equal("value::10", cache["key::10"]);
			Assert.Equal("value::50", cache["key::50"]);
			Assert.Equal("value::55", cache["key::55"]);
		}

		[Fact]
		public void CanMassTimeout()
		{
			var cache = new MemoryCache<string, string>();
			var values = new List<Caching.Abstract.KeyValuePair<string, string>>();
			var options = new CacheEntryOptions {
				Timeout = TimeSpan.FromMilliseconds(50)
			};

			for(var idx = 0; idx < 100; idx++) {
				var kvp = new Caching.Abstract.KeyValuePair<string, string> {
					Key = $"key::{idx}",
					Value = $"value::{idx}"
				};

				values.Add(kvp);
			}

			cache.AddOrUpdate(values, options);
			Assert.Equal("value::50", cache["key::50"]);
			Thread.Sleep(51);
			Assert.Throws<ArgumentOutOfRangeException>(() => cache["key::50"]);
		}
	}
}
