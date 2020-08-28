/*
 * Redis memory cache implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using StackExchange.Redis;

using SensateService.Common.Caching.Abstract;
using SensateService.Common.Caching.Memory;

using Generics = System.Collections.Generic;

namespace SensateService.Common.Caching.Redis
{
	public class RedisCache<TValue> : IDistributedCache<TValue> where TValue : class
	{
		private readonly DistributedCacheOptions m_options;
		private readonly bool m_gzip;

		protected readonly ISystemClock m_clock;
		protected readonly SemaphoreSlim m_networkLock;
		protected IDatabase m_database;
		private IConnectionMultiplexer m_connectionMultiplexer;

		public RedisCache(IOptions<DistributedCacheOptions> options)
		{
			if(options == null) {
				throw new ArgumentNullException(nameof(options), "Options cannot be null!");
			}

			this.m_options = options.Value;
			this.m_gzip = options.Value.Gzip;
			this.m_clock = new SystemClock();
			this.m_networkLock = new SemaphoreSlim(1, 1);
		}

		~RedisCache()
		{
			this.Dispose();
		}

		private async Task ConnectAsync(CancellationToken ct)
		{
			ct.ThrowIfCancellationRequested();

			if(this.m_database != null) {
				return;
			}

			await this.m_networkLock.WaitAsync(ct).ConfigureAwait(false);

			try {
				if(this.m_database == null) {
					this.m_connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(this.m_options.Configuration).ConfigureAwait(false);
					this.m_database = this.m_connectionMultiplexer.GetDatabase();
				}
			} finally {
				this.m_networkLock.Release();
			}
		}

		public async Task SetAsync(string key, TValue value, CacheEntryOptions options = null, CancellationToken ct = default)
		{
			this.ValidateKey(key);
			this.ValidateValue(value);
			ct.ThrowIfCancellationRequested();

			await this.ConnectAsync(ct).ConfigureAwait(false);
			await this.m_networkLock.WaitAsync(ct).ConfigureAwait(false);

			try {
				var rvalue = this.Serialize(value);

				if(options?.Timeout != null) {
					await this.m_database.StringSetAsync(key, rvalue, options.Timeout).ConfigureAwait(false);
				} else {
					await this.m_database.StringSetAsync(key, rvalue).ConfigureAwait(false);
				}
			} finally {
				this.m_networkLock.Release();
			}
		}

		public async Task<TValue> GetAsync(string key, CancellationToken ct = default)
		{
			TValue result;

			this.ValidateKey(key);
			ct.ThrowIfCancellationRequested();

			await this.ConnectAsync(ct).ConfigureAwait(false);
			await this.m_networkLock.WaitAsync(ct).ConfigureAwait(false);

			try {
				var results = await this.m_database.StringGetAsync(key).ConfigureAwait(false);

				if(!results.HasValue) {
					throw new ArgumentOutOfRangeException(nameof(key), "Key not found in cache!");
				}

				result = this.Deserialize(results);
			} finally {
				this.m_networkLock.Release();
			}

			return result;
		}

		public async Task RemoveAsync(string key, CancellationToken ct = default)
		{
			this.ValidateKey(key);
			ct.ThrowIfCancellationRequested();

			await this.ConnectAsync(ct).ConfigureAwait(false);
			await this.m_networkLock.WaitAsync(ct).ConfigureAwait(false);

			try {
				await this.m_database.KeyDeleteAsync(new RedisKey(key)).ConfigureAwait(false);
			} finally {
				this.m_networkLock.Release();
			}
		}

		public async Task SetRangeAsync(ICollection<Abstract.KeyValuePair<string, TValue>> values, CacheEntryOptions options = null, CancellationToken ct = default)
		{
			foreach(var kvp in values) {
				this.ValidateKey(kvp.Key);
				this.ValidateValue(kvp.Value);
			}

			ct.ThrowIfCancellationRequested();

			await this.ConnectAsync(ct).ConfigureAwait(false);
			await this.m_networkLock.WaitAsync(ct).ConfigureAwait(false);

			try {
				var kvp = values.Select(k => new Generics.KeyValuePair<RedisKey, RedisValue>(
											new RedisKey(k.Key), this.Serialize(k.Value)
				));
				await this.m_database.StringSetAsync(kvp.ToArray()).ConfigureAwait(false);
			} finally {
				this.m_networkLock.Release();
			}

		}

		public async Task<IEnumerable<TValue>> GetRangeAsync(ICollection<string> keys, CancellationToken ct = default)
		{
			RedisValue[] results;
			foreach(var key in keys) {
				this.ValidateKey(key);
			}

			ct.ThrowIfCancellationRequested();

			await this.m_networkLock.WaitAsync(ct).ConfigureAwait(false);

			try {
				var redisKeys = keys.Select(k => new RedisKey(k));
				await this.ConnectAsync(ct).ConfigureAwait(false);
				results = await this.m_database.StringGetAsync(redisKeys.ToArray()).ConfigureAwait(false);
			} finally {
				this.m_networkLock.Release();
			}

			return results.Select(this.Deserialize);
		}

		protected virtual void ValidateKey(string key)
		{
			if(string.IsNullOrWhiteSpace(key)) {
				throw new ArgumentNullException(nameof(key), "Key should not be null or empty!");
			}
		}

		protected virtual void ValidateValue(TValue value)
		{
			if(value == null) {
				throw new ArgumentNullException(nameof(value), "Cannot store a null value!");
			}
		}

		private static byte[] GetBytes(object value)
		{
			var str = JsonConvert.SerializeObject(value, Formatting.None);
			return Encoding.UTF8.GetBytes(str);
		}

		private static TValue GetObject(byte[] bytes)
		{
			var json = Encoding.UTF8.GetString(bytes);
			return JsonConvert.DeserializeObject<TValue>(json);
		}

		public static byte[] Decompress(byte[] input)
		{
			using(var source = new MemoryStream(input)) {
				byte[] lengthBytes = new byte[4];
				source.Read(lengthBytes, 0, 4);

				var length = BitConverter.ToInt32(lengthBytes, 0);
				using(var decompressionStream = new GZipStream(source,
															   CompressionMode.Decompress)) {
					var result = new byte[length];
					decompressionStream.Read(result, 0, length);
					return result;
				}
			}
		}

		public static byte[] Compress(byte[] input)
		{
			using(var result = new MemoryStream()) {
				var lengthBytes = BitConverter.GetBytes(input.Length);
				result.Write(lengthBytes, 0, 4);

				using(var compressionStream = new GZipStream(result,
															 CompressionMode.Compress)) {
					compressionStream.Write(input, 0, input.Length);
					compressionStream.Flush();

				}
				return result.ToArray();
			}
		}

		private RedisValue Serialize(TValue value)
		{
			string str;

			if(typeof(TValue) == typeof(string)) {
				str = value as string ?? "";

				if(this.m_gzip) {
					var bytes = Encoding.UTF8.GetBytes(str);
					bytes = Compress(bytes);
					str = Convert.ToBase64String(bytes);
				}
			} else {
				var bytes = GetBytes(value);

				if(this.m_gzip) {
					bytes = Compress(bytes);
				}

				str = Convert.ToBase64String(bytes);
			}

			return new RedisValue(str);
		}

		private TValue Deserialize(RedisValue results)
		{
			TValue result;

			if(typeof(TValue) == typeof(string)) {
				if(this.m_gzip) {
					var str = results.ToString();
					var bytes = Convert.FromBase64String(str);

					bytes = Decompress(bytes);
					result = Encoding.UTF8.GetString(bytes) as TValue;
				} else {
					result = results.ToString() as TValue;
				}
			} else {
				var bytes = Convert.FromBase64String(results.ToString());

				if(this.m_gzip) {
					bytes = Decompress(bytes);
				}

				result = GetObject(bytes);
			}

			return result;
		}

		protected virtual void Dispose(bool disposing)
		{
			if(disposing) {
				this.m_networkLock?.Dispose();
				this.m_connectionMultiplexer?.Dispose();
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
