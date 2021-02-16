/*
 * Unit tests for MemoryCache<string, string>.Add().
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
	public class AddRangeTests
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

			cache.Add(values);
			Assert.AreEqual("value::50", cache["key::50"]);
		}

		[TestMethod]
		public void CannotMassUpdate()
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

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => cache.Add(values));
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

			cache.Add(values, options);
			Assert.AreEqual("value::50", cache["key::50"]);
			Thread.Sleep(51);
			Assert.ThrowsException<KeyNotFoundException>(() => cache["key::50"]);
		}

	}
}