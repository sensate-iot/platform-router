/*
 * Get/set data from a redis cache.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using SensateIoT.Common.Caching.Abstract;
using SensateIoT.Common.Caching.Redis;

using StackExchange.Redis;

namespace SensateIoT.Common.Caching.LoadTest.DistributedTests
{
	public class TestData
	{
		public long LongValue { get; set; }
		public string StrValue { get; set; }
	}

	public class GetSetTests
	{
		public async Task Run()
		{
			await ValidateSetGetAsync().ConfigureAwait(false);
			await ValidateTimeout().ConfigureAwait(false);
			await ValidateRangeGet().ConfigureAwait(false);
			await ValidateRangeSet().ConfigureAwait(false);
			await ValidateRemove().ConfigureAwait(false);
		}

		private static async Task ValidateSetGetAsync()
		{
			var opts = new DistributedCacheOptions {
				Configuration = new ConfigurationOptions {
					EndPoints = { { "127.0.0.1", 6379 } },
					ClientName = "dbg-client"
				}
			};

			var cache = new RedisCache<TestData>(opts);
			var data = new TestData {
				LongValue = 10000,
				StrValue = "Hello, World"
			};

			await cache.SetAsync("abc", data).ConfigureAwait(false);
			var result = await cache.GetAsync("abc").ConfigureAwait(false);

			if(result.StrValue != data.StrValue || result.LongValue != data.LongValue) {
				throw new SystemException("Invalid cache read back!");
			}
		}

		private static async Task ValidateRangeGet()
		{
			var opts = new DistributedCacheOptions {
				Configuration = new ConfigurationOptions {
					EndPoints = { { "127.0.0.1", 6379 } }, ClientName = "dbg-client"
				},
				Gzip = true
			};

			var cache = new RedisCache<TestData>(opts);

			var d1 = new TestData {
				LongValue = 10001,
				StrValue = "Hello, World 1"
			};

			var d2 = new TestData {
				LongValue = 10002,
				StrValue = "Hello, World 2"
			};

			var d3 = new TestData {
				LongValue = 10003,
				StrValue = "Hello, World 3"
			};

			await Task.WhenAll(
				cache.SetAsync("k1", d1),
				cache.SetAsync("k2", d2),
				cache.SetAsync("k3", d3)
			);

			var entries = new List<string> { "k1", "k2", "k3" };
			var tmp = await cache.GetRangeAsync(entries).ConfigureAwait(false);
			var results = tmp.ToList();

			if(results[0].StrValue != "Hello, World 1" || results[1].LongValue != 10002) {
				throw new SystemException("Invalid read back from server!");
			}
		}

		private static async Task ValidateRemove()
		{
			var opts = new DistributedCacheOptions {
				Configuration = new ConfigurationOptions {
					EndPoints = { { "127.0.0.1", 6379 } }, ClientName = "dbg-client"
				},
				Gzip = true
			};

			var cache = new RedisCache<TestData>(opts);

			var data = new TestData {
				LongValue = 10000,
				StrValue = "Hello, World"
			};

			await cache.SetAsync("rm-key", data).ConfigureAwait(false);

			var result = await cache.GetAsync("rm-key").ConfigureAwait(false);

			if(result.StrValue != data.StrValue || result.LongValue != data.LongValue) {
				throw new SystemException("Invalid cache read back!");
			}

			await cache.RemoveAsync("rm-key").ConfigureAwait(false);

			try {
				await cache.GetAsync("rm-key").ConfigureAwait(false);
				throw new ApplicationException("Expected an ArgumentOutOfRangeException!");
			} catch(ArgumentOutOfRangeException) {

			}
		}

		private static async Task ValidateRangeSet()
		{
			var opts = new DistributedCacheOptions {
				Configuration = new ConfigurationOptions {
					EndPoints = { { "127.0.0.1", 6379 } }, ClientName = "dbg-client"
				},
				Gzip = true
			};

			var cache = new RedisCache<TestData>(opts);
			var entries = new List<Abstract.KeyValuePair<string, TestData>> {
				new Abstract.KeyValuePair<string, TestData> {
					Key = "p1",
					Value = new TestData {
						LongValue = 10001,
						StrValue = "Hello, World 1"
					}
				},
				new Abstract.KeyValuePair<string, TestData> {
					Key = "p2",
					Value = new TestData {
						LongValue = 10002,
						StrValue = "Hello, World 2"
					}
				},
				new Abstract.KeyValuePair<string, TestData> {
					Key = "p3",
					Value = new TestData {
						LongValue = 10003,
						StrValue = "Hello, World 3"
					}
				},
			};

			await cache.SetRangeAsync(entries).ConfigureAwait(false);
			var keys = new List<string> { "p1", "p2", "p3" };
			var tmp = await cache.GetRangeAsync(keys).ConfigureAwait(false);
			var results = tmp.ToList();

			if(results[0].StrValue != "Hello, World 1" || results[1].LongValue != 10002) {
				throw new SystemException("Invalid read back from server!");
			}
		}

		private static async Task ValidateTimeout()
		{
			var opts = new DistributedCacheOptions {
				Configuration = new ConfigurationOptions {
					EndPoints = { { "127.0.0.1", 6379 } },
					ClientName = "dbg-client"
				}
			};

			var cache = new RedisCache<TestData>(opts);
			var data = new TestData {
				LongValue = 10000,
				StrValue = "Hello, World"
			};

			var options = new CacheEntryOptions {
				Timeout = TimeSpan.FromSeconds(1)
			};

			await cache.SetAsync("abc", data, options).ConfigureAwait(false);
			Thread.Sleep(1250);

			try {
				await cache.GetAsync("abc").ConfigureAwait(false);
				throw new SystemException("Key should not be found but is found!");
			} catch(ArgumentOutOfRangeException) {
			}
		}
	}
}
