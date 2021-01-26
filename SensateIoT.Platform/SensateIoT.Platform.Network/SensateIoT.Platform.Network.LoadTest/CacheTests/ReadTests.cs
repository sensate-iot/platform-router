/*
 * Memory cache reat tests.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Memory;

namespace SensateIoT.Platform.Network.LoadTest.CacheTests
{
	public class ReadTests
	{
		private const int DefaultTimeout = 5 * 60 * 1000;
		private readonly IList<ObjectId> m_ids;

		public ReadTests()
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

		public void TestSynchronousRead(int size)
		{
			Console.WriteLine($"Starting synchronous lookup test with {size} items!");
			var cache = this.BuildTestCache(size);

			var ms = this.TestReads(cache);

			var perItem = ms * 1000 * 1000 / size;
			Console.WriteLine($"Lookup of {size} items took {ms}ms");
			Console.WriteLine($"Lookup per item {perItem}ns");
		}

		public void TestAsynchronousRead(int size)
		{
			Console.WriteLine($"Starting asynchronous lookup test with {size * 4} items!");
			var cache = this.BuildTestCache(size);

			long durations = 0;

			var t1 = new Thread(() => {
				var result = this.TestReads(cache);
				Interlocked.Add(ref durations, result);
			});
			var t2 = new Thread(() => {
				var result = this.TestReads(cache);
				Interlocked.Add(ref durations, result);
			});
			var t3 = new Thread(() => {
				var result = this.TestReads(cache);
				Interlocked.Add(ref durations, result);
			});
			var t4 = new Thread(() => {
				var result = this.TestReads(cache);
				Interlocked.Add(ref durations, result);
			});

			t1.Start();
			t2.Start();
			t3.Start();
			t4.Start();

			t1.Join();
			t2.Join();
			t3.Join();
			t4.Join();

			var ms = durations / 4;

			var perItem = ms * 1000 * 1000 / (size * 4);
			Console.WriteLine($"Lookup of {size * 4} items took {ms}ms");
			Console.WriteLine($"Lookup per item {perItem}ns");
		}

		private long TestReads(object obj)
		{
			var cache = obj as IMemoryCache<ObjectId, string>;
			var sw = Stopwatch.StartNew();

			int idx = 0;

			foreach(var id in this.m_ids) {
				if(!cache.TryGetValue(id, out var value)) {
					throw new AggregateException("Unable to find expected key!");
				}

				if(value != $"value::{idx}") {
					throw new DataException($"Expected and actual value do not match. Expected: value::{idx}. Actual: {value}.");
				}

				idx++;
			}

			sw.Stop();

			return sw.ElapsedMilliseconds;
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
