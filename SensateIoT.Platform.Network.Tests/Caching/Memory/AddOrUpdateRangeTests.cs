/*
 * Add or update range unit tests.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Memory;

namespace SensateIoT.Platform.Network.Tests.Caching.Memory
{
	[TestClass]
	public class AddOrUpdateRangeTests
	{
		[TestMethod]
		public void CanAddMultipleKVPairs()
		{
			var cache = new MemoryCache<string, string>();
			var values = new List<Common.Caching.Abstract.KeyValuePair<string, string>>();

			for(var idx = 0; idx < 100; idx++) {
				var kvp = new Common.Caching.Abstract.KeyValuePair<string, string> {
					Key = $"key::{idx}",
					Value = $"value::{idx}"
				};

				values.Add(kvp);
			}

			cache.AddOrUpdate(values);
			Assert.AreEqual("value::50", cache["key::50"]);
		}

		[TestMethod]
		public void CanAddAndUpdate()
		{
			var cache = new MemoryCache<string, string>();
			var values = new List<Common.Caching.Abstract.KeyValuePair<string, string>>();

			cache["key::10"] = "abc";
			cache["key::50"] = "def";
			cache["key::55"] = "duf";

			Assert.AreEqual("abc", cache["key::10"]);
			Assert.AreEqual("def", cache["key::50"]);
			Assert.AreEqual("duf", cache["key::55"]);

			for(var idx = 0; idx < 100; idx++) {
				var kvp = new Common.Caching.Abstract.KeyValuePair<string, string> {
					Key = $"key::{idx}",
					Value = $"value::{idx}"
				};

				values.Add(kvp);
			}

			cache.AddOrUpdate(values);
			Assert.AreEqual("value::10", cache["key::10"]);
			Assert.AreEqual("value::50", cache["key::50"]);
			Assert.AreEqual("value::55", cache["key::55"]);
		}

		[TestMethod]
		public void CanMassTimeout()
		{
			var cache = new MemoryCache<string, string>();
			var values = new List<Common.Caching.Abstract.KeyValuePair<string, string>>();
			var options = new CacheEntryOptions {
				Timeout = TimeSpan.FromMilliseconds(50)
			};

			for(var idx = 0; idx < 100; idx++) {
				var kvp = new Common.Caching.Abstract.KeyValuePair<string, string> {
					Key = $"key::{idx}",
					Value = $"value::{idx}"
				};

				values.Add(kvp);
			}

			cache.AddOrUpdate(values, options);
			Assert.AreEqual("value::50", cache["key::50"]);
			Thread.Sleep(51);
			Assert.ThrowsException<KeyNotFoundException>(() => cache["key::50"]);
		}
	}
}
