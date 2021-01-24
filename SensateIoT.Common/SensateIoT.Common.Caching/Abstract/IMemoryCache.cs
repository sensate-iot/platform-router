/*
 * Memory caching interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace SensateIoT.Common.Caching.Abstract
{
	/// <summary>
	/// Memory cache interface.
	/// </summary>
	/// <typeparam name="TKey">Key type.</typeparam>
	/// <typeparam name="TValue">Value type.</typeparam>
	[PublicAPI]
	public interface IMemoryCache<TKey, TValue> : IDisposable
	{
		/// <summary>
		/// Attempt to get the value associated with <i>key</i>.
		/// </summary>
		/// <param name="key">Key to search for.</param>
		/// <param name="value">Output parameter to store the value associated with <i>key</i>.</param>
		/// <returns>True or false based on whether <i>key</i> was found or not.</returns>
		/// <exception cref="ObjectDisposedException">When the memory cache instance is disposed.</exception>
		/// <exception cref="ArgumentNullException">When <i>key</i> is <i>null</i>.</exception>
		/// <exception cref="CacheException">When the cache entry behind <i>key</i> is in an invalid state.</exception>
		bool TryGetValue(TKey key, out TValue value);

		/// <summary>
		/// Attempt to get the value associated with <i>key</i>.
		/// </summary>
		/// <param name="key">Key to search for.</param>
		/// <returns>The value associated with <i>key</i>.</returns>
		/// <exception cref="ObjectDisposedException">When the memory cache instance is disposed.</exception>
		/// <exception cref="ArgumentNullException">When <i>key</i> is <i>null</i>.</exception>
		/// <exception cref="CacheException">When the cache entry behind <i>key</i> is in an invalid state.</exception>
		/// <exception cref="ArgumentOutOfRangeException">When <i>key</i> doesn't exist</exception>
		TValue this[TKey key] { get; set; }

		/// <summary>
		/// Add a value to the cache, or update the current entry if the given key already exists in the cache.
		/// </summary>
		/// <param name="key">Key to add or update.</param>
		/// <param name="value">Value to set.</param>
		/// <param name="options">Cache entry options.</param>
		/// <exception cref="ArgumentOutOfRangeException">If the <i>value</i> is too big.</exception>
		/// <exception cref="ObjectDisposedException">When the memory cache instance is disposed.</exception>
		/// <exception cref="ArgumentNullException">When the key is <i>null</i>.</exception>
		/// <exception cref="CacheException">When the cache entry behind <i>key</i> is in an invalid state.</exception>
		void AddOrUpdate(TKey key, TValue value, CacheEntryOptions options = null);

		/// <summary>
		/// Add a new cache key/value pair to the cache.
		/// </summary>
		/// <param name="key">Key to add.</param>
		/// <param name="value">Value to add.</param>
		/// <param name="options">Cache entry configuration.</param>
		/// <exception cref="ArgumentOutOfRangeException">If the <i>value</i> is too big.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If the key already exists.</exception>
		/// <exception cref="ObjectDisposedException">When the memory cache instance is disposed.</exception>
		/// <exception cref="ArgumentNullException">When the key is <i>null</i>.</exception>
		void Add(TKey key, TValue value, CacheEntryOptions options = null);

		/// <summary>
		/// Add a new cache key/value pair to the cache.
		/// </summary>
		/// <param name="values">Key-value pairs to add to the cache.</param>
		/// <param name="options">Cache entry configuration.</param>
		/// <exception cref="ArgumentOutOfRangeException">If the <i>value</i> is too big.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If the key already exists.</exception>
		/// <exception cref="ObjectDisposedException">When the memory cache instance is disposed.</exception>
		/// <exception cref="ArgumentNullException">When the key is <i>null</i>.</exception>
		void Add(IEnumerable<KeyValuePair<TKey, TValue>> values, CacheEntryOptions options = null);

		/// <summary>
		/// Add or update a range of key-value pairs.
		/// </summary>
		/// <param name="values">Key-value pairs to add to the memory cache.</param>
		/// <param name="options">Cache entry options.</param>
		/// <exception cref="ArgumentOutOfRangeException">If the <i>value</i> is too big.</exception>
		/// <exception cref="ObjectDisposedException">When the memory cache instance is disposed.</exception>
		/// <exception cref="ArgumentNullException">When the values array is <i>null</i>.</exception>
		/// <exception cref="CacheException">When the cache entry behind <i>key</i> is in an invalid state.</exception>
		void AddOrUpdate(IEnumerable<KeyValuePair<TKey, TValue>> values, CacheEntryOptions options = null);

		/// <summary>
		/// Remove an entry from the memory cache.
		/// </summary>
		/// <param name="key">Key to remove</param>
		/// <exception cref="ArgumentOutOfRangeException">When the key is not found.</exception>
		/// <exception cref="CacheException">When the cache entry behind <i>key</i> is in an invalid state.</exception>
		/// <exception cref="ArgumentNullException">When the key is <i>null</i>.</exception>
		/// <exception cref="ObjectDisposedException">When the memory cache instance is disposed.</exception>
		void Remove(TKey key);

		/// <summary>
		/// Attempt to remove a key.
		/// </summary>
		/// <param name="key"></param>
		/// <exception cref="ArgumentNullException">When the key is <i>null</i>.</exception>
		/// <exception cref="CacheException">When the cache entry behind <i>key</i> is in an invalid state.</exception>
		/// <exception cref="ObjectDisposedException">When the memory cache instance is disposed.</exception>
		/// <returns>True or false based on whether the removal of <i>key</i> was successful.</returns>
		bool TryRemove(TKey key);

		/// <summary>
		/// Scan the cache for expired entries.
		/// </summary>
		void ScanForExpiredItems();

		/// <summary>
		/// Remove entries that have been scheduled for removal.
		/// </summary>
		void RemoveScheduledEntries();
	}
}
