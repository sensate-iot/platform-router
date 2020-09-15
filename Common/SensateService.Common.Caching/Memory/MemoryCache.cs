/*
 * In-memory cache implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using SensateService.Common.Caching.Abstract;
using SensateService.Common.Caching.Internal;

[assembly: InternalsVisibleTo("SensateService.Common.Caching.Tests")]
namespace SensateService.Common.Caching.Memory
{
	public class MemoryCache<TKey, TValue> : IMemoryCache<TKey, TValue>
	{
		private bool m_disposed;
		protected readonly ISystemClock m_clock;

		private SpinLock m_deletionLock;
		private DateTimeOffset m_lastScan;
		private IList<TKey> m_deletionQueue;

		protected readonly ReaderWriterLockSlim m_dataLock;
		protected readonly IDictionary<TKey, CacheEntry<TValue>> m_data;
		private readonly int m_timeout;
		private readonly long m_capacity;
		private long m_size;

		public const int DefaultTimeout = 30 * 60 * 1000; // 30 minutes in milliseconds
		public const long DefaultCapacity = -1L;
		private static readonly TimeSpan ScanTimeout = TimeSpan.FromMinutes(1);

		public int Count {
			get {
				this.m_dataLock.EnterReadLock();
				var rv = this.m_data.Count;
				this.m_dataLock.ExitReadLock();

				return rv;
			}
		}

		public long Size {
			get {
				this.m_dataLock.EnterReadLock();
				var rv = this.m_size;
				this.m_dataLock.ExitReadLock();

				return rv;
			}
		}

		public long Capacity {
			get {
				this.m_dataLock.EnterReadLock();
				var rv = this.m_capacity;
				this.m_dataLock.ExitReadLock();

				return rv;
			}
		}

		/// <summary>
		/// Create a default memory cache.
		/// </summary>
		public MemoryCache() : this(DefaultCapacity)
		{
		}

		/// <summary>
		/// Create a memory cache with a specific capacity and timeout.
		/// </summary>
		/// <param name="capacity">Capacity for this instance.</param>
		/// <param name="timeout">Entry timeout.</param>
		public MemoryCache(long capacity, int timeout = DefaultTimeout) : this(capacity, timeout, new SystemClock())
		{
		}

		/// <summary>
		/// Create a memory cache with a specific capacity, timeout and clock implementation.
		/// </summary>
		/// <param name="capacity">Capacity for this instance.</param>
		/// <param name="timeout">Entry timeout.</param>
		/// <param name="clock">System clock implementation.</param>
		public MemoryCache(long capacity, int timeout, ISystemClock clock)
		{
			this.m_disposed = false;
			this.m_timeout = timeout;
			this.m_clock = clock;
			this.m_data = new Dictionary<TKey, CacheEntry<TValue>>();
			this.m_dataLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
			this.m_lastScan = DateTimeOffset.MinValue;
			this.m_deletionQueue = new List<TKey>();
			this.m_deletionLock = new SpinLock();
			this.m_capacity = capacity;
			this.m_size = 0L;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "Reviewed")]
		~MemoryCache()
		{
			this.Dispose();
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}"/>
		public TValue this[TKey key] {
			get {
				if(!this.TryGetValue(key, out var value)) {
					throw new ArgumentOutOfRangeException(nameof(key), $"Key {key} not found in the cache!");
				}

				return value;
			}

			set => this.AddOrUpdate(key, value);
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}"/>
		public bool TryGetValue(TKey key, out TValue value)
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

				this.ValidateCacheEntry(entry);
				var now = this.m_clock.GetUtcNow();
				var expiry = entry.CreatedAt.AddMilliseconds(entry.Timeout);

				if(expiry >= now && entry.State == EntryState.None) {
					entry.LastSeen = now;
					value = entry.Value;
				} else {
					rv = false;
					value = default;

					if(entry.State == EntryState.None || entry.State == EntryState.Expired) {
						this.ScheduleForRemoval(key, entry);
						this.StartDeletionScan();
					}
				}
			} finally {
				this.m_dataLock.ExitReadLock();
			}

			return rv;
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}"/>
		public void AddOrUpdate(TKey key, TValue value, CacheEntryOptions options = null)
		{
			this.CheckDisposed();
			this.ValidateCacheKey(key, false);
			this.ValidateCacheEntryOptions(options);

			this.m_dataLock.EnterWriteLock();

			try {
				this.InternalAddOrUpdate(key, value, options);
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Add or update a cache entry. This method assumes that <i>m_dataLock</i> is already
		/// acquired in write mode.
		/// </summary>
		/// <param name="key">Key to add or update.</param>
		/// <param name="value">Value to add.</param>
		/// <param name="options">Insert options.</param>
		private void InternalAddOrUpdate(TKey key, TValue value, CacheEntryOptions options)
		{
			var oldSize = -1L;
			bool update;

			if(this.m_data.TryGetValue(key, out var entry)) {
				this.ValidateCacheEntry(entry);

				oldSize = entry.Size;
				update = true;
				this.m_size -= entry.Size;
			} else {
				entry = this.BuildCacheEntry(value, options);
				update = false;
			}

			if(!this.ValidateAndUpdateCacheCapacity(entry)) {
				entry.Size = oldSize;
				throw new ArgumentOutOfRangeException(
					nameof(value),
					$"Value to big to store in cache. Current size is {this.m_size}, " +
					$"attempting to add an entry of size {entry.Size} would overflow the cache."
				);
			}

			if(update && entry.State != EntryState.Removed) {
				entry.Timeout = DefaultTimeout;

				if(options?.Timeout != null) {
					entry.Timeout = options.GetTimeoutMs();
				}

				entry.Value = value;
				entry.LastSeen = this.m_clock.GetUtcNow();
				entry.CreatedAt = this.m_clock.GetUtcNow();
				entry.State = EntryState.None;
				entry.Size = options?.Size ?? 0L;
			} else {
				this.m_data.Add(key, entry);
			}
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}"/>
		public void Add(TKey key, TValue value, CacheEntryOptions options = null)
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

				if(!this.ValidateAndUpdateCacheCapacity(entry)) {
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

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}"/>
		public void Add(IEnumerable<Abstract.KeyValuePair<TKey, TValue>> values, CacheEntryOptions options = null)
		{
			this.CheckDisposed();
			this.ValidateCacheEntryOptions(options);
			// ReSharper disable once PossibleMultipleEnumeration
			ValidateKeyValuePairs(values);

			// ReSharper disable once PossibleMultipleEnumeration
			foreach(var pair in values) {
				this.ValidateCacheKey(pair.Key, true);
				this.m_dataLock.EnterWriteLock();

				try {
					this.InternalAddOrUpdate(pair.Key, pair.Value, options);
				} finally {
					this.m_dataLock.ExitWriteLock();
				}
			}

		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}"/>
		public void AddOrUpdate(IEnumerable<Abstract.KeyValuePair<TKey, TValue>> values, CacheEntryOptions options = null)
		{
			this.CheckDisposed();
			this.ValidateCacheEntryOptions(options);
			// ReSharper disable once PossibleMultipleEnumeration
			ValidateKeyValuePairs(values);

			// ReSharper disable once PossibleMultipleEnumeration
			foreach(var pair in values) {
				this.ValidateCacheKey(pair.Key, false);
				this.m_dataLock.EnterWriteLock();

				try {
					this.InternalAddOrUpdate(pair.Key, pair.Value, options);
				} finally {
					this.m_dataLock.ExitWriteLock();
				}
			}
		}

		private CacheEntry<TValue> BuildCacheEntry(TValue value, CacheEntryOptions options)
		{
			var timeout = this.m_timeout;

			if(options?.Timeout != null) {
				timeout = options.GetTimeoutMs();
			}

			return new CacheEntry<TValue> {
				State = EntryState.None,
				CreatedAt = this.m_clock.GetUtcNow(),
				Timeout = timeout,
				LastSeen = this.m_clock.GetUtcNow(),
				Value = value,
				Size = options?.Size ?? 0L
			};
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}"/>
		public void Remove(TKey key)
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

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}"/>
		public bool TryRemove(TKey key)
		{
			bool result;

			this.CheckDisposed();
			this.ValidateCacheKey(key, false);

			this.m_dataLock.EnterReadLock();

			try {
				if(!this.m_data.TryGetValue(key, out var entry)) {
					return false;
				}

				this.ValidateCacheEntry(entry);

				if(entry.State == EntryState.ScheduledForRemoval) {
					result = false;
				} else {
					this.ScheduleForRemoval(key, entry);
					this.StartDeletionScan();
					result = true;
				}
			} finally {
				this.m_dataLock.ExitReadLock();
			}

			return result;
		}

		/// <summary>
		/// Attempt to schedule a deletion scan.
		/// </summary>
		protected void StartDeletionScan()
		{
			var taken = false;

			this.m_deletionLock.Enter(ref taken);

			if(!taken) {
				throw new CacheException("Unable to obtain spin-lock!");
			}

			try {
				var now = this.m_clock.GetUtcNow();
				var expiry = this.m_lastScan.Add(ScanTimeout);

				if(now < expiry) {
					return;
				}

				this.m_lastScan = now;
			} finally {
				this.m_deletionLock.Exit();
			}

			Task.Factory.StartNew(this.RemoveScheduledEntries,
								  default,
								  TaskCreationOptions.DenyChildAttach,
								  TaskScheduler.Default);
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}"/>
		public void RemoveScheduledEntries()
		{
			var taken = false;
			IList<TKey> data = new List<TKey>();

			this.m_deletionLock.Enter(ref taken);

			if(!taken) {
				throw new CacheException("Unable to acquire spin lock!");
			}

			try {
				var tmp = this.m_deletionQueue;
				this.m_deletionQueue = data;
				data = tmp;
			} finally {
				this.m_deletionLock.Exit();
			}

			this.m_dataLock.EnterWriteLock();

			try {
				foreach(var key in data) {
					if(!this.m_data.TryGetValue(key, out var entry)) {
						continue;
					}

					if(entry.State == EntryState.ScheduledForRemoval) {
						this.m_data.Remove(key);
					}
				}
			} finally {
				this.m_dataLock.ExitWriteLock();
			}
		}

		/// <inheritdoc cref="IMemoryCache{TKey,TValue}"/>
		public void ScanForExpiredItems()
		{
			var taken = false;

			this.m_dataLock.EnterReadLock();
			this.m_deletionLock.Enter(ref taken);

			if(!taken) {
				this.m_dataLock.ExitReadLock();
				throw new CacheException("Unable to acquire deletion lock!");
			}

			try {
				var now = this.m_clock.GetUtcNow();

				foreach(var kvp in this.m_data) {
					if(kvp.Value.CreatedAt.AddMilliseconds(kvp.Value.Timeout) <= now) {
						this.ScheduleForRemovalLocked(kvp.Key, kvp.Value);
					}
				}
			} finally {
				this.m_deletionLock.Exit();
				this.m_dataLock.ExitReadLock();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Reviewed")]
		private void ScheduleForRemoval(TKey key, CacheEntry<TValue> entry)
		{
			var taken = false;
			this.m_deletionLock.Enter(ref taken);

			if(!taken) {
				throw new SystemException("Unable to acquire spin lock!");
			}

			try {
				this.ScheduleForRemovalLocked(key, entry);
			} finally {
				this.m_deletionLock.Exit();
			}
		}

		private void ScheduleForRemovalLocked(TKey key, CacheEntry<TValue> entry)
		{
			entry.State = EntryState.ScheduledForRemoval;
			this.m_deletionQueue.Add(key);
		}

		/// <summary>
		/// This method assumes that the dictionary lock is acquired in a write state.
		/// </summary>
		/// <param name="entry">Entry to validate.</param>
		private bool ValidateAndUpdateCacheCapacity(CacheEntry<TValue> entry)
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

		protected virtual void ValidateCacheEntry(CacheEntry<TValue> entry)
		{
			if(entry?.State == EntryState.Removed) {
				throw new CacheException("Removed cache entry still in cache!");
			}
		}

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

				var expiry = value.CreatedAt.AddMilliseconds(value.Timeout);

				if(expiry > this.m_clock.GetUtcNow()) {
					throw new ArgumentOutOfRangeException(nameof(key), $"Key ({key}) already exists in cache!");
				}
			} finally {
				this.m_dataLock.ExitReadLock();
			}
		}

		protected virtual void ValidateCacheEntryOptions(CacheEntryOptions options)
		{
			if(this.m_capacity > 0L && (options?.Size == null || options.Size.Value <= 0L)) {
				throw new ArgumentException("Invalid entry options. Size required!", nameof(options));
			}
		}

		protected virtual void CheckDisposed()
		{
			if(!this.m_disposed) {
				return;
			}

			throw new ObjectDisposedException(typeof(MemoryCache<TKey, TValue>).FullName);
		}

		[AssertionMethod]
		private static void ValidateKeyValuePairs(IEnumerable<Abstract.KeyValuePair<TKey, TValue>> values)
		{
			if(values == null) {
				throw new ArgumentNullException(nameof(values), "Cannot add a range of null!");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "Reviewed.")]
		protected virtual void Dispose(bool disposing)
		{
			if(this.m_disposed) {
				return;
			}

			if(disposing) {
				this.m_dataLock.Dispose();
				GC.SuppressFinalize(this);
			}

			this.m_disposed = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "Reviewed.")]
		public void Dispose()
		{
			this.Dispose(true);
		}
	}
}
