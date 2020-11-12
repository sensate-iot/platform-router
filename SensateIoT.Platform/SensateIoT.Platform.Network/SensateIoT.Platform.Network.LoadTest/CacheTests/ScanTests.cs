/*
 * Test timeout scanning.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using MongoDB.Bson;
using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Memory;

namespace SensateIoT.Platform.Network.LoadTest.CacheTests
{
	public class ScanTests
	{
		private const int DefaultTimeout = 5 * 60 * 1000;
		private readonly IList<ObjectId> m_ids;

		public ScanTests()
		{
			this.m_ids = new List<ObjectId>();
		}

		public void GenerateIds(int size)
		{
			var time = DateTime.Now;

			for(var i = 0; i < size; i++) {
				this.m_ids.Add(ObjectId.GenerateNewId(time));
			}
		}

		public void ScanForTimeouts(int count)
		{
			var cache = this.BuildTestCache(count);

			Console.WriteLine($"Scanning for timeouts on a cache with {cache.Count} items.");

			var sw = Stopwatch.StartNew();
			cache.ScanForExpiredItems();
			sw.Stop();

			Console.WriteLine($"Scanning took {sw.ElapsedMilliseconds}ms for {cache.Count} items.");
		}

		private IMemoryCache<ObjectId, string> BuildTestCache(int size)
		{
			var cache = new MemoryCache<ObjectId, string>(MemoryCache<ObjectId, string>.DefaultCapacity, TimeSpan.FromMilliseconds(DefaultTimeout));
			var kvp = new List<Common.Caching.Abstract.KeyValuePair<ObjectId, string>>(size);

			for(var idx = 0; idx < size; idx++) {
				kvp.Add(new Common.Caching.Abstract.KeyValuePair<ObjectId, string> {
					Key = this.m_ids[idx],
					Value = $"value::{idx}"
				});
			}

			cache.Add(kvp);
			return cache;
		}
	}
}
