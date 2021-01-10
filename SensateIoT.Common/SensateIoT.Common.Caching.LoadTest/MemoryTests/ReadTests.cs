/*
 * Memory cache read tests.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using SensateIoT.Common.Caching.Abstract;
using SensateIoT.Common.Caching.Memory;

namespace SensateIoT.Common.Caching.LoadTest.MemoryTests
{
	public class ReadTests
	{
		private const int DefaultTimeout = 5 * 60 * 1000;

		public void TestSynchronousRead(int size)
		{
			Console.WriteLine($"Starting synchronous lookup test with {size} items!");
			var cache = BuildTestCache(size);

			var ms = TestReads(cache, size);

			var perItem = ms * 1000 * 1000 / size;
			Console.WriteLine($"Lookup of {size} items took {ms}ms");
			Console.WriteLine($"Lookup per item {perItem}ns");
		}

		public async Task TestAsynchronousRead(int size)
		{
			Console.WriteLine($"Starting asynchronous lookup test with {size * 4} items!");
			var cache = BuildTestCache(size);

			var t1 = Task.Run(() => TestReads(cache, size));
			var t2 = Task.Run(() => TestReads(cache, size));
			var t3 = Task.Run(() => TestReads(cache, size));
			var t4 = Task.Run(() => TestReads(cache, size));

			var durations = await Task.WhenAll(t1, t2, t3, t4).ConfigureAwait(false);
			var ms = durations.Sum() / 4;

			var perItem = ms * 1000 * 1000 / (size * 4);
			Console.WriteLine($"Lookup of {size * 4} items took {ms}ms");
			Console.WriteLine($"Lookup per item {perItem}ns");
		}

		private static long TestReads(IMemoryCache<string, string> cache, int size)
		{
			var sw = Stopwatch.StartNew();

			for(var idx = size - 1; idx >= 0; idx--) {
				if(!cache.TryGetValue($"key::{idx}", out _)) {
					throw new AggregateException("Unable to find expected key!");
				}
			}

			sw.Stop();

			return sw.ElapsedMilliseconds;
		}

		private static IMemoryCache<string, string> BuildTestCache(int size)
		{
			var cache = new MemoryCache<string, string>(MemoryCache<string, string>.DefaultCapacity, DefaultTimeout);
			var kvp = new List<Abstract.KeyValuePair<string, string>>(size);

			for(var idx = 0; idx < size; idx++) {
				kvp.Add(new Abstract.KeyValuePair<string, string> {
					Key = $"key::{idx}",
					Value = $"value::{idx}"
				});
			}

			cache.Add(kvp);
			return cache;
		}
	}
}
