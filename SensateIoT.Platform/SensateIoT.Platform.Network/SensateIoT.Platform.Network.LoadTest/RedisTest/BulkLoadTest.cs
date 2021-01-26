/*
 * Redis memory cache implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using MongoDB.Bson;
using StackExchange.Redis;

using SensateIoT.Platform.Network.Common.Caching.Abstract;

namespace SensateIoT.Platform.Network.LoadTest.RedisTest
{
	public class BulkLoadTest
	{
		private readonly DistributedCacheOptions m_options;
		private readonly bool m_gzip;

		protected IDatabase m_database;
		private IConnectionMultiplexer m_connectionMultiplexer;

		public BulkLoadTest(IOptions<DistributedCacheOptions> options)
		{
			if(options == null) {
				throw new ArgumentNullException(nameof(options), "Options cannot be null!");
			}

			this.m_options = options.Value;
			this.m_gzip = options.Value.Gzip;
		}

		private async Task ConnectAsync(CancellationToken ct)
		{
			ct.ThrowIfCancellationRequested();

			if(this.m_database != null) {
				return;
			}

			if(this.m_database == null) {
				this.m_connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(this.m_options.Configuration)
					.ConfigureAwait(false);
				this.m_database = this.m_connectionMultiplexer.GetDatabase();
			}
		}

		public async Task SetAsync(string key, string value, CacheEntryOptions options = null, CancellationToken ct = default)
		{
			this.ValidateKey(key);

			ct.ThrowIfCancellationRequested();

			if(!this.m_connectionMultiplexer.IsConnected) {
				await this.ConnectAsync(ct).ConfigureAwait(false);
			}

			await this.m_database.StringSetAsync(key, value, null, When.Always, CommandFlags.FireAndForget).ConfigureAwait(false);
		}

		protected virtual void ValidateKey(string key)
		{
			if(string.IsNullOrWhiteSpace(key)) {
				throw new ArgumentNullException(nameof(key), "Key should not be null or empty!");
			}
		}

		public void Run(int count)
		{
			var list = new List<Dictionary<ObjectId, string>> {
				new Dictionary<ObjectId, string>(),
				new Dictionary<ObjectId, string>(),
				new Dictionary<ObjectId, string>(),
				new Dictionary<ObjectId, string>(),
				new Dictionary<ObjectId, string>()
			};

			var size = count / 5;
			var last = count % 5;

			for(var idx = 0; idx < 5; idx++) {
				var num = idx == 4 ? last : size;

				for(var j = 0; j < num; j++) {
					var id = ObjectId.GenerateNewId();
					var uuid1 = Guid.NewGuid();
					var uuid2 = Guid.NewGuid();

					list[idx].Add(id, $"{uuid1:D}::{uuid2:D}");
				}
			}

			var connect = this.ConnectAsync(default);
			connect.Wait();

			var sw = Stopwatch.StartNew();

			var opts = new ParallelOptions {
				MaxDegreeOfParallelism = 4
			};

			foreach(var dict in list) {
				var result = Parallel.ForEach(dict, opts, async (pair) => {
					await this.SetAsync(pair.Key.ToString(), pair.Value).ConfigureAwait(false);
				});

				if(result.IsCompleted) {
					Console.WriteLine("Completed bulk write!");
				}
			}

			sw.Stop();
			Console.WriteLine($"Finished bulk load test in {sw.ElapsedMilliseconds}ms");
		}
	}
}
