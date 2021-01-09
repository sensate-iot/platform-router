/*
 * Unit tests for IMemoryCache<>.Clear().
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Memory;

namespace SensateIoT.Platform.Network.Tests.Caching.Memory
{
	[TestClass]
	public class ClearTests
	{
		private static IMemoryCache<string, string> GenerateCache()
		{
			var cache = new MemoryCache<string, string>(150);
			var values = new List<Common.Caching.Abstract.KeyValuePair<string, string>>();
			var options = new CacheEntryOptions { Size = 1 };

			for(var idx = 0; idx < 100; idx++) {
				var kvp = new Common.Caching.Abstract.KeyValuePair<string, string> {
					Key = $"key::{idx}",
					Value = $"value::{idx}"
				};

				values.Add(kvp);
			}

			cache.Add(values, options);
			return cache;
		}

		[TestMethod]
		public void ClearRemovesAllEntries()
		{
			var cache = GenerateCache();

			Assert.AreEqual(100, cache.Size);
			Assert.IsTrue(cache.Count > 0);

			cache.Clear();

			Assert.AreEqual(0, cache.Size);
			Assert.AreEqual(0, cache.Count);
		}
	}
}
