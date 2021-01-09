/*
 * Type safe in-memory cache implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Internal;
using SensateIoT.Platform.Network.Common.Exceptions;
using SensateIoT.Platform.Network.Common.Helpers;

namespace SensateIoT.Platform.Network.Common.Caching.Memory
{
	/// <summary>
	/// Represents an in-memory cache.
	/// </summary>
	/// <typeparam name="TKey">Key type.</typeparam>
	/// <typeparam name="TValue">Value type.</typeparam>
	[PublicAPI]
	[DebuggerDisplay("Count = {Count}, Capacity = {Capacity}, Size = {Size}, DefaultTimeout = {m_defaultTimeout}")]
	[DebuggerTypeProxy(typeof(MemoryCache<,>.DebugViewer))]
	public class MemoryCache<TKey, TValue> : IMemoryCache<TKey, TValue>
	{
		private readonly ISystemClock m_clock;
		private bool m_disposed;
		private readonly CancellationTokenSource m_cts;

		private SpinLockWrapper m_deletionLock;
		private DateTimeOffset m_lastScan;
		private IList<TKey> m_deletionQueue;

		protected readonly ReaderWriterLockSlim m_dataLock;
		protected readonly IDictionary<TKey, CacheEntry<TValue>> m_data;
		private readonly TimeSpan m_defaultTimeout;
		private long m_capacity;
		private long m_size;
		private bool m_activeScanning;

		/// <summary>
		/// Default timeout.
		/// </summary>
		public const int StaticDefaultTimeout = 5 * 60 * 1000;

		/// <summary>
		/// Default memory cache capacity. This capacity acts as an infinite capacity.
		/// </summary>
		public const long DefaultCapacity = -1L; // Infinite capacity

		private static readonly TimeSpan ScanTimeout = TimeSpan.FromMinutes(1);

		/// <summary>
		/// Create new memory cache.
		/// </summary>
		public MemoryCache() : this(DefaultCapacity)
		{
		}

		/// <summary>
		/// Create a new memory cache.
		/// </summary>
		/// <param name="capacity">Cache capacity to use.</param>
		public MemoryCache(long capacity) : this(capacity, TimeSpan.FromMinutes(StaticDefaultTimeout))
		{
		}

		/// <summary>
		/// Create a new memory cache.
		/// </summary>
		/// <param name="capacity">Cache capacity.</param>
		/// <param name="timeout">Default entry timeout.</param>
		public MemoryCache(long capacity, TimeSpan timeout) : this(capacity, timeout, new SystemClock())
		{
		}

		/// <summary>
		/// Create a new memory cache.
		/// </summary>
		/// <param name="capacity">Cache capacity.</param>
		/// <param name="timeout">Default entry timeout.</param>
		/// <param name="clock">Cache clock to compute entry times and timeouts.</param>
		public MemoryCache(long capacity, TimeSpan timeout, ISystemClock clock)
		{
			this.m_defaultTimeout = timeout;
			this.m_clock = clock;
			this.m_capacity = capacity;

			this.m_activeScanning = true;
			this.m_disposed = false;
			this.m_data = new Dictionary<TKey, CacheEntry<TValue>>();
			this.m_dataLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
			this.m_lastScan = DateTimeOffset.MinValue;
			this.m_deletionQueue = new List<TKey>();
			this.m_deletionLock = new SpinLockWrapper();
			this.m_size = 0L;
			this.m_cts = new CancellationTokenSource();
		}

		~MemoryCache()
		{
			this.Dispose();
		}

		public bool ActiveTimeoutScanningEnabled {
			get {
				this.m_dataLock.EnterReadLock();
				var rv = this.m_activeScanning;
				this.m_dataLock.ExitReadLock();
				return rv;
			}

			set {
				this.m_dataLock.EnterWriteLock();
				this.m_activeScanning = value;
				this.m_dataLock.ExitWriteLock();
			}
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}.Count"/>
		public int Count {
			get {
				this.m_dataLock.EnterReadLock();
				var rv = this.m_data.Count;
				this.m_dataLock.ExitReadLock();
				return rv;
			}
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}.Size"/>
		public long Size {
			get {
				this.m_dataLock.EnterReadLock();
				var rv = this.m_size;
				this.m_dataLock.ExitReadLock();
				return rv;
			}
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}.Capacity"/>
		public long Capacity {
			get {
				this.m_dataLock.EnterReadLock();
				var rv = this.m_capacity;
				this.m_dataLock.ExitReadLock();
				return rv;
			}

			set {
				this.m_dataLock.EnterWriteLock();
				this.m_capacity = value;
				this.m_dataLock.ExitWriteLock();
			}
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}.DefaultTimeout"/>
		public TimeSpan DefaultTimeout {
			get {
				this.m_dataLock.EnterReadLock();
				var rv = this.m_defaultTimeout;
				this.m_dataLock.ExitReadLock();

				return rv;
			}
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}.this"/>
		public TValue this[TKey key] {
			get {
				if(!this.TryGetValue(key, out var value)) {
					throw new KeyNotFoundException($"Key not found: {key}");
				}

				return value;
			}

			set => this.AddOrUpdate(key, value);
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}.TryGetValue"/>
		public virtual bool TryGetValue(TKey key, out TValue value)
		{
			bool rv;

			this.CheckDisposed();
			this.ValidateCacheKey(key, false);

			this.m_dataLock.EnterReadLock();

			try {
				rv = this.m_data.TryGetValue(key, out var entry);

				if(!rv) {
					value = default;
					return false;
				}

				this.ValidateCacheEntry(key, entry);

				var now = this.m_clock.GetUtcNow();
				var expiry = entry.Timestamp.Add(entry.Timeout);

				if(expiry > now && entry.State == EntryState.None) {
					value = entry.Value;
				} else {
					rv = false;
					value = default;

					if(entry.State == EntryState.None || entry.State == EntryState.Expired) {
						entry.State = EntryState.Expired;
						this.ScheduleForRemovalIf(key, entry);
					}
				}
			} finally {
				this.m_dataLock.ExitReadLock();
			}

			return rv;
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}.AddOrUpdate(TKey,TValue,CacheEntryOptions)"/>
		public virtual void AddOrUpdate(TKey key, TValue value, CacheEntryOptions options = null)
		{
			this.CheckDisposed();
			this.ValidateCacheKey(key, false);
			this.ValidateCacheEntryOptions(options);

			this.m_dataLock.EnterReadLock();

			try {
				this.InternalAddOrUpdate(key, value, options);
			} finally {
				this.m_dataLock.ExitReadLock();
			}
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}.Add(TKey,TValue,CacheEntryOptions)"/>
		public virtual void Add(TKey key, TValue value, CacheEntryOptions options = null)
		{
			this.CheckDisposed();
			this.ValidateCacheKey(key, true);
			this.ValidateCacheEntryOptions(options);

			this.m_dataLock.EnterWriteLock();

			try {
				if(this.m_data.ContainsKey(key)) {
					/*
					 * The key already exists, but not in an active state. In
					 * this case we can just overwrite it.
					 */
					this.InternalAddOrUpdate(key, value, options);
					return;
				}

				this.ValidateCacheEntryOptions(options);
				var entry = this.BuildCacheEntry(value, options);

				if(!this.ValidateAndUpdateCacheSize(entry)) {
					throw new ArgumentOutOfRangeException(
						nameof(value),
						$"Value too big to store in cache. Current size is {this.m_size}, " +
						$"attempting to add entry of size {entry.Size} would overflow the cache."
					);
				}

				this.m_data.Add(key, entry);
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}.Add(TKey,TValue,CacheEntryOptions)"/>
		public virtual void Add(IEnumerable<Abstract.KeyValuePair<TKey, TValue>> values, CacheEntryOptions options = null)
		{
			this.CheckDisposed();
			this.ValidateCacheEntryOptions(options);
			// ReSharper disable once PossibleMultipleEnumeration
			ValidateKeyValuePairs(values);
			this.m_dataLock.EnterWriteLock();

			try {
				// ReSharper disable once PossibleMultipleEnumeration
				foreach(var pair in values) {
					this.ValidateCacheKey(pair.Key, true);
					this.InternalAddOrUpdate(pair.Key, pair.Value, options);
				}
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}.AddOrUpdate(TKey,TValue,CacheEntryOptions)"/>
		public virtual void AddOrUpdate(IEnumerable<Abstract.KeyValuePair<TKey, TValue>> values, CacheEntryOptions options = null)
		{
			this.CheckDisposed();
			this.ValidateCacheEntryOptions(options);
			// ReSharper disable once PossibleMultipleEnumeration
			ValidateKeyValuePairs(values);

			this.m_dataLock.EnterWriteLock();

			try {
				// ReSharper disable once PossibleMultipleEnumeration
				foreach(var pair in values) {
					this.ValidateCacheKey(pair.Key, false);
					this.InternalAddOrUpdate(pair.Key, pair.Value, options);
				}
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}.Remove"/>
		public virtual void Remove(TKey key)
		{
			this.CheckDisposed();
			this.ValidateCacheKey(key, false);

			this.m_dataLock.EnterReadLock();

			try {
				if(!this.m_data.ContainsKey(key)) {
					throw new ArgumentOutOfRangeException(
						nameof(key),
						$"Unable to remove {key} from cache (key doesn't exist)!"
					);
				}

				if(!this.TryRemove(key)) {
					throw new ArgumentOutOfRangeException(
						nameof(key),
						$"Unable to remove {key} from cache (key not found)!"
					);
				}
			} finally {
				this.m_dataLock.ExitReadLock();
			}
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}.TryRemove"/>
		public virtual bool TryRemove(TKey key)
		{
			bool result;

			this.CheckDisposed();
			this.ValidateCacheKey(key, false);

			this.m_dataLock.EnterReadLock();

			try {
				if(!this.m_data.TryGetValue(key, out var entry)) {
					return false;
				}

				this.ValidateCacheEntry(key, entry);

				if(entry.State == EntryState.ScheduledForRemoval) {
					result = false;
				} else {
					this.ScheduleForRemoval(key, entry);

					if(this.m_activeScanning) {
						this.StartDeletionScan();
					}

					result = true;
				}
			} finally {
				this.m_dataLock.ExitReadLock();
			}

			return result;
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}.Clear"/>
		public virtual void Clear()
		{
			this.CheckDisposed();
			this.m_deletionLock.Lock();

			try {
				this.m_deletionQueue.Clear();
			} finally {
				this.m_deletionLock.Unlock();
			}

			this.m_dataLock.EnterWriteLock();

			try {
				this.m_data.Clear();
				this.m_size = 0;
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}.ScanForExpiredItems"/>
		public virtual void ScanForExpiredItems()
		{
			this.CheckDisposed();

			this.m_dataLock.EnterReadLock();
			this.m_deletionLock.Lock();

			try {
				var now = this.m_clock.GetUtcNow();
				var dict = new ConcurrentBag<System.Collections.Generic.KeyValuePair<TKey, CacheEntry<TValue>>>();

				Parallel.ForEach(this.m_data, pair => {
					if(pair.Value.State == EntryState.Expired || pair.Value.Timestamp.Add(pair.Value.Timeout) <= now) {
						dict.Add(pair);
					}
				});

				foreach(var (key, value) in dict) {
					this.ScheduleForRemovalLocked(key, value);
				}
			} finally {
				this.m_deletionLock.Unlock();
				this.m_dataLock.ExitReadLock();
			}
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}.RemoveScheduledEntries"/>
		public void RemoveScheduledEntries()
		{
			IList<TKey> data = new List<TKey>();

			this.CheckDisposed();
			this.m_deletionLock.Lock();

			try {
				var tmp = this.m_deletionQueue;
				this.m_deletionQueue = data;
				data = tmp;
			} finally {
				this.m_deletionLock.Unlock();
			}

			this.m_dataLock.EnterWriteLock();

			try {
				foreach(var key in data) {
					if(!this.m_data.TryGetValue(key, out var entry)) {
						continue;
					}

					if(entry.State == EntryState.ScheduledForRemoval || entry.State == EntryState.Expired) {
						this.m_data.Remove(key);
					}
				}
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}.RemoveScheduledEntriesAsync"/>
		public Task RemoveScheduledEntriesAsync()
		{
			this.CheckDisposed();
			return this.StartDeletionScan();
		}

		/// <summary>
		/// Validate a cache key. If <i>throwIfExists</i> is set, this method will also check
		/// if <i>key</i> is already in the cache.
		/// </summary>
		/// <param name="key">Key to check.</param>
		/// <param name="throwIfExists">Test if <i>key</i> is already in the memory cache.</param>
		/// <exception cref="ArgumentNullException">If <i>key</i> is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If key already exists in the cache, and <i>throwIfExists</i> is set to <b>true</b>.</exception>
		[AssertionMethod]
		protected virtual void ValidateCacheKey(TKey key, bool throwIfExists)
		{
			if(key == null) {
				throw new ArgumentNullException(nameof(key));
			}

			if(!throwIfExists) {
				return;
			}

			this.m_dataLock.EnterReadLock();

			try {
				if(!this.m_data.TryGetValue(key, out var value) || value.State != EntryState.None) {
					return;
				}

				var expiry = value.Timestamp.Add(value.Timeout);

				if(expiry > this.m_clock.GetUtcNow()) {
					throw new ArgumentOutOfRangeException(nameof(key), $"Key ({key}) already exists in cache!");
				}
			} finally {
				this.m_dataLock.ExitReadLock();
			}
		}

		/// <summary>
		/// Validate the state of a cache entry.
		/// </summary>
		/// <param name="key">The key of <i>entry</i>.</param>
		/// <param name="entry">Entry to test.</param>
		/// <exception cref="CacheException">If <i>entry</i> has an invalid state.</exception>
		[AssertionMethod]
		protected virtual void ValidateCacheEntry(TKey key, CacheEntry<TValue> entry)
		{
			if(entry?.State == EntryState.Removed) {
				throw new CacheException(key, "Removed entry still in cache!");
			}
		}

		/// <summary>
		/// Validate <i>CacheEntryOptions</i>.
		/// </summary>
		/// <param name="options">Options to validate.</param>
		/// <exception cref="ArgumentException">If the options are invalid.</exception>
		[AssertionMethod]
		protected virtual void ValidateCacheEntryOptions(CacheEntryOptions options)
		{
			if(this.m_capacity > 0L && (options?.Size == null || options.Size.Value <= 0L)) {
				throw new ArgumentException("Invalid entry options. Size required!", nameof(options));
			}
		}

		/// <summary>
		/// Attempt to schedule a deletion scan.
		/// </summary>
		protected Task StartDeletionScan()
		{
			this.m_deletionLock.Lock();

			try {
				var now = this.m_clock.GetUtcNow();
				var expiry = this.m_lastScan.Add(ScanTimeout);

				if(now < expiry) {
					return Task.CompletedTask;
				}

				this.m_lastScan = now;
			} finally {
				this.m_deletionLock.Unlock();
			}

			return Task.Factory.StartNew(this.RemoveScheduledEntries,
										 this.m_cts.Token,
										 TaskCreationOptions.DenyChildAttach,
										 TaskScheduler.Default);
		}

		/// <summary>
		/// Add or update a cache entry. This method assumes that <i>m_dataLock</i> is already
		/// acquired in write mode.
		/// </summary>
		/// <param name="key">Key to add or update.</param>
		/// <param name="value">Value to add.</param>
		/// <param name="options">Insert options.</param>
		protected void InternalAddOrUpdate(TKey key, TValue value, CacheEntryOptions options)
		{
			var oldSize = -1L;
			bool update;

			if(this.m_data.TryGetValue(key, out var entry)) {
				this.ValidateCacheEntry(key, entry);

				oldSize = entry.Size;
				update = true;
				this.m_size -= entry.Size;
			} else {
				entry = this.BuildCacheEntry(value, options);
				update = false;
			}

			if(!this.ValidateAndUpdateCacheSize(entry)) {
				entry.Size = oldSize;
				throw new ArgumentOutOfRangeException(
					nameof(value),
					$"Value to big to store in cache. Current size is {this.m_size}, " +
					$"attempting to add an entry of size {entry.Size} would overflow the cache."
				);
			}

			if(update && entry.State != EntryState.Removed) {
				entry.Timeout = this.m_defaultTimeout;

				if(options?.Timeout != null) {
					entry.Timeout = options.Timeout.Value;
				}

				entry.Value = value;
				entry.Timestamp = this.m_clock.GetUtcNow();
				entry.State = EntryState.None;
				entry.Size = options?.Size ?? 0L;
			} else {
				this.m_data.Add(key, entry);
			}
		}

		/// <summary>
		/// Schedule a key for removal.
		/// </summary>
		/// <param name="key">Key schedule for removal.</param>
		/// <param name="entry">Entry belonging to <i>key</i>.</param>
		private void ScheduleForRemovalLocked(TKey key, CacheEntry<TValue> entry)
		{
			entry.State = EntryState.ScheduledForRemoval;
			this.m_deletionQueue.Add(key);
		}

		/// <summary>
		/// Build a new cache entry.
		/// </summary>
		/// <param name="value">Entry value.</param>
		/// <param name="options">Cache entry options.</param>
		/// <returns></returns>
		private CacheEntry<TValue> BuildCacheEntry(TValue value, CacheEntryOptions options)
		{
			var timeout = this.m_defaultTimeout;

			if(options?.Timeout != null) {
				timeout = options.Timeout.Value;
			}

			return new CacheEntry<TValue> {
				State = EntryState.None,
				Timestamp = this.m_clock.GetUtcNow(),
				Timeout = timeout,
				Value = value,
				Size = options?.Size ?? 0L
			};
		}

		/// <summary>
		/// This method assumes that the dictionary lock is acquired in a write state.
		/// </summary>
		/// <param name="entry">Entry to validate.</param>
		private bool ValidateAndUpdateCacheSize(CacheEntry<TValue> entry)
		{
			var newSize = this.m_size + entry.Size;

			if(this.m_capacity < 0L) {
				return true;
			}

			if(newSize > this.m_capacity) {
				return false;
			}

			this.m_size = newSize;
			return true;
		}

		/// <summary>
		/// Check if the cache has been disposed.
		/// </summary>
		/// <exception cref="ObjectDisposedException"></exception>
		[AssertionMethod]
		private void CheckDisposed()
		{
			if(this.m_disposed) {
				throw new ObjectDisposedException(typeof(MemoryCache<TKey, TValue>).FullName);
			}
		}

		/// <summary>
		/// Schedule an object for removal only if active scanning is enabled.
		/// </summary>
		/// <param name="key">Key to schedule for removal.</param>
		/// <param name="entry">Entry that belongs to <i>key</i>.</param>
		private void ScheduleForRemovalIf(TKey key, CacheEntry<TValue> entry)
		{
			if(this.m_activeScanning) {
				this.ScheduleForRemoval(key, entry);
			}
		}

		/// <summary>
		/// Schedule an object for removal.
		/// </summary>
		/// <param name="key">Key to schedule for removal.</param>
		/// <param name="entry">Entry that belongs to <i>key</i>.</param>
		private void ScheduleForRemoval(TKey key, CacheEntry<TValue> entry)
		{
			this.m_deletionLock.Lock();

			try {
				this.ScheduleForRemovalLocked(key, entry);
			} finally {
				this.m_deletionLock.Unlock();
			}
		}

		/// <summary>
		/// Validate a KVP array.
		/// </summary>
		/// <param name="values">Array to validate.</param>
		[AssertionMethod]
		protected static void ValidateKeyValuePairs(IEnumerable<Abstract.KeyValuePair<TKey, TValue>> values)
		{
			if(values == null) {
				throw new ArgumentNullException(nameof(values), "Cannot add a range of null!");
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if(disposing) {
				this.Clear();
				this.m_cts.Cancel();
				this.m_dataLock.Dispose();
				this.m_cts.Dispose();
			}

			this.m_disposed = true;
		}

		/// <inheritdoc cref="IDisposable.Dispose"/>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		[DebuggerNonUserCode]
		private sealed class DebugViewer
		{
			private readonly MemoryCache<TKey, TValue> m_cache;

			[UsedImplicitly]
			public IDictionary<TKey, TValue> Entries {
				get {
					return this.m_cache.m_data.ToDictionary(
							k => k.Key,
							v => v.Value.Value
						);
				}
			}


			public DebugViewer(MemoryCache<TKey, TValue> cache)
			{
				this.m_cache = cache;
			}
		}
	}
}
