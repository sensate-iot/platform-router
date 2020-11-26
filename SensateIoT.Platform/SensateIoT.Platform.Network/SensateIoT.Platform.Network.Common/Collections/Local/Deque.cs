/*
 * Concurrent and generic deque implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using JetBrains.Annotations;

using SensateIoT.Platform.Network.Common.Collections.Abstract;
using SensateIoT.Platform.Network.Common.Exceptions;
using SensateIoT.Platform.Network.Common.Helpers;

namespace SensateIoT.Platform.Network.Common.Collections.Local
{
	[PublicAPI]
	public class Deque<TValue> : IQueue<TValue>
	{
		private const int DefaultCapacity = 256;

		protected SpinLockWrapper m_lock;
		protected TValue[] m_data;
		protected int m_count;

		private int m_offset;
		private int m_isDisposed;

		public bool IsFixedSize => false;
		public bool IsReadOnly => false;
		public bool IsSynchronized => true;
		public object SyncRoot => this;

		public Deque() : this(DefaultCapacity)
		{
		}

		public Deque(int capacity)
		{
#if DEBUG
			this.m_lock = new SpinLockWrapper(true);
#else
			this.m_lock = new SpinLockWrapper(false);
#endif

			this.m_isDisposed = 0;
			this.m_count = 0;
			this.m_offset = 0;
			this.m_data = null;
			this.SetCapacity(capacity);
		}

		public Deque(IEnumerable<TValue> collection)
		{
			if(collection == null) {
				throw new ArgumentNullException(nameof(collection), "Collection cannot be null.");
			}
#if DEBUG
			this.m_lock = new SpinLockWrapper(true);
#else
			this.m_lock = new SpinLockWrapper(false);
#endif

			var source = CollectionExtensions.ToReadOnlyCollection(collection);
			var count = source.Count;

			if(count > 0) {
				this.SetCapacity(count);
				this.DoAddRange(source);
			} else {
				this.m_data = new TValue[DefaultCapacity];
				this.m_count = 0;
				this.m_offset = 0;
			}

			this.m_isDisposed = 0;
		}

		~Deque()
		{
			this.Dispose(false);
		}

		public int Count {
			get {
				int rv;

				this.CheckDisposed();
				this.m_lock.Lock();
				rv = this.m_count;
				this.m_lock.Unlock();

				return rv;
			}
		}

		public int Capacity {
			get {
				int rv;

				this.CheckDisposed();
				this.m_lock.Lock();

				rv = this.m_data.Length;
				this.m_lock.Unlock();

				return rv;
			}

			set {
				this.CheckDisposed();
				this.m_lock.Lock();

				try {
					this.SetCapacity(value);
				} finally {
					this.m_lock.Unlock();
				}
			}
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			var array = this.ToArray();

			foreach(var value in array) {
				yield return value;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public void CopyTo(TValue[] items, int index)
		{
			this.CopyTo(items as Array, index);
		}

		public void CopyTo(Array array, int index)
		{
			this.CheckDisposed();
			this.m_lock.Lock();

			try {
				this.CheckRangeArgument(array.Length, index);
				this.CopyToArray(array, index);
			} finally {
				this.m_lock.Unlock();
			}
		}

		public virtual void AddFront(TValue item)
		{
			this.CheckDisposed();
			this.m_lock.Lock();

			try {
				this.PushFront(item);
			} finally {
				this.m_lock.Unlock();
			}
		}

		public virtual void AddRangeFront(IEnumerable<TValue> items)
		{
			if(items == null) {
				throw new ArgumentNullException(nameof(items), "Collection cannot be null.");
			}

			this.CheckDisposed();
			var result = CollectionExtensions.ToReadOnlyCollection(items);

			if(result.Count <= 0) {
				return;
			}

			this.m_lock.Lock();

			try {
				this.EnsureCapacity(result.Count);
				var idx = this.PreDecrement(result.Count);
				this.CopyFrom(result, idx);
			} finally {
				this.m_lock.Unlock();
			}
		}

		public virtual void Add(TValue item)
		{
			this.CheckDisposed();
			this.m_lock.Lock();

			try {
				this.PushBack(item);
			} finally {
				this.m_lock.Unlock();
			}
		}

		public virtual void AddRange(IEnumerable<TValue> items)
		{
			if(items == null) {
				throw new ArgumentNullException(nameof(items), "Collection cannot be null.");
			}

			this.CheckDisposed();
			this.m_lock.Lock();

			try {
				this.DoAddRange(items);
			} finally {
				this.m_lock.Unlock();
			}
		}

		public virtual TValue Dequeue()
		{
			TValue returnValue;

			this.CheckDisposed();
			this.m_lock.Lock();

			try {
				this.VerifyQueueSize();
				var idx = this.PostIncrement(1);

				returnValue = this.m_data[idx];
				this.m_count -= 1;
			} finally {
				this.m_lock.Unlock();
			}

			return returnValue;
		}

		public IEnumerable<TValue> DequeueRange(int count)
		{
			TValue[] result;

			this.CheckDisposed();
			this.VerifyDequeueCount(count);
			this.m_lock.Lock();

			try {
				if(this.m_count <= 0) {
					return new List<TValue>();
				}

				count = Math.Min(count, this.m_count);
				result = new TValue[count];

				if(this.IsSplit()) {
					var partition = Math.Min(this.m_data.Length - this.m_offset, count);
					var remaining = count;

					Array.Copy(this.m_data, this.m_offset, result, 0, partition);
					Array.Clear(this.m_data, this.m_offset, partition);

					remaining -= partition;

					if(remaining > 0) {
						Array.Copy(this.m_data, 0, result, partition, remaining);
						Array.Clear(this.m_data, 0, remaining);
					}
				} else {
					Array.Copy(this.m_data, this.m_offset, result, 0, count);
				}

				this.m_offset = (this.m_offset + count) & (this.m_data.Length - 1);
				this.m_count -= count;
			} finally {
				this.m_lock.Unlock();
			}

			return result;
		}

		public TValue this[int index] {
			get => this.GetByIndex(index);
			set => this.SetByIndex(index, value);
		}

		public TValue Peek(int index)
		{
			return this.GetByIndex(index);
		}

		public bool Contains(TValue item)
		{
			var comparer = EqualityComparer<TValue>.Default;

			this.CheckDisposed();
			return this.Any(entry => comparer.Equals(item, entry));
		}

		public void Clear()
		{
			this.CheckDisposed();
			this.m_lock.Lock();

			try {
				if(this.m_count <= 0) {
					return;
				}

				if(this.IsSplit()) {
					var partition = this.m_data.Length - this.m_offset;

					Array.Clear(this.m_data, this.m_offset, this.m_data.Length);
					partition = this.m_count - partition;
					Array.Clear(this.m_data, 0, partition);
				} else {
					var partition = this.m_offset + this.m_count;
					Array.Clear(this.m_data, this.m_offset, partition);
				}

				this.m_offset = 0;
				this.m_count = 0;

				this.SetCapacity(DefaultCapacity);
			} finally {
				this.m_lock.Unlock();
			}
		}

		public void Trim()
		{
			this.m_lock.Lock();

			/*
			 * Trim if:
			 *  - count < 2^(n-1);
			 *  - capacity * 0.9 > count;
			 */

			try {
				var capacity = this.FindSmallestCapacity();

				if(this.m_data.Length == capacity) {
					return;
				}

				this.SetCapacity(capacity);
			} finally {
				this.m_lock.Unlock();
			}
		}

		public TValue[] ToArray()
		{
			TValue[] array;

			this.CheckDisposed();
			this.m_lock.Lock();

			try {
				array = new TValue[this.m_count];
				this.CopyToArray(array);
			} finally {
				this.m_lock.Unlock();
			}

			return array;
		}

		public bool TryAdd(TValue item)
		{
			bool rv;
			this.m_lock.Lock();

			try {
				this.CheckDisposed();
				this.PushBack(item);
				rv = true;
			} catch(ArgumentOutOfRangeException) {
				rv = false;
			} catch(ObjectDisposedException) {
				rv = false;
			} finally {
				this.m_lock.Unlock();
			}

			return rv;
		}

		public bool TryTake(out TValue item)
		{
			bool rv;

			try {
				this.CheckDisposed();
				item = this.Dequeue();
				rv = true;
			} catch(ObjectDisposedException) {
				item = default;
				rv = false;
			} catch(UnderflowException) {
				item = default;
				rv = false;
			}

			return rv;
		}

		protected int GetArrayIndex(int idx)
		{
			return (idx + this.m_offset) & (this.m_data.Length - 1);
		}

		protected TValue GetByIndex(int index)
		{
			TValue result;

			this.CheckDisposed();
			this.m_lock.Lock();

			try {
				result = this.DoGetByIndex(index);
			} finally {
				this.m_lock.Unlock();
			}

			return result;
		}

		protected TValue DoGetByIndex(int index)
		{
			this.CheckIfIndexExists(index);
			var idx = this.GetArrayIndex(index);
			return this.m_data[idx];
		}

		protected void SetByIndex(int index, TValue value)
		{
			this.CheckDisposed();
			this.m_lock.Lock();

			try {
				this.CheckIfIndexExists(index);
				var idx = this.GetArrayIndex(index);
				this.m_data[idx] = value;
			} finally {
				this.m_lock.Unlock();
			}
		}

		private void PushFront(TValue item)
		{
			this.EnsureCapacity(1);

			var idx = this.PreDecrement(1);
			this.m_data[idx] = item;
			this.m_count += 1;
		}

		private int PushBack(TValue item)
		{
			this.EnsureCapacity(1);

			var idx = this.GetArrayIndex(this.m_count);
			this.m_data[idx] = item;
			var rv = this.m_count;
			this.m_count += 1;

			return rv;
		}

		private void CopyToArray(Array array, int index = 0)
		{
			if(array == null) {
				throw new ArgumentNullException(nameof(array), "Destination array should not be null.");
			}

			if(this.IsSplit()) {
				var length = this.m_data.Length - this.m_offset;

				Array.Copy(this.m_data, this.m_offset, array, index, length);
				Array.Copy(this.m_data, 0, array, index + length, this.m_count - length);
			} else {
				Array.Copy(this.m_data, this.m_offset, array, index, this.m_count);
			}
		}

		private void CopyFrom(IReadOnlyCollection<TValue> values, int targetIndex)
		{
			foreach(var value in values) {
				var idx = this.GetArrayIndex(targetIndex);
				this.m_data[idx] = value;
				targetIndex++;
			}

			this.m_count += values.Count;
		}

		private void CheckIfIndexExists(int index)
		{
			if(index < 0 || index >= this.m_count) {
				throw new ArgumentOutOfRangeException(nameof(index),
													  $"Invalid index {index} for queue length {this.m_count}.");
			}
		}

		private int PreDecrement(int count)
		{
			if(this.m_offset < count) {
				this.m_offset += this.m_data.Length - count;
			} else {
				this.m_offset -= count;
			}

			return this.m_offset;
		}

		private int PostIncrement(int count)
		{
			var rv = this.m_offset;

			this.m_offset += count;
			this.m_offset &= this.m_data.Length - 1;

			return rv;
		}

		private void VerifyQueueSize()
		{
			if(this.m_count <= 0) {
				throw new UnderflowException("Unable to perform operation on an empty Deque.");
			}
		}

		private void VerifyDequeueCount(int count)
		{
			if(count <= 0) {
				throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");
			}
		}

		private void CheckRangeArgument(int length, int offset)
		{
			if(offset < 0) {
				throw new ArgumentOutOfRangeException(nameof(offset), $"Invalid offset: {offset}.");
			}

			if(length < 0) {
				throw new ArgumentOutOfRangeException(nameof(length), $"Invalid length: {length}.");
			}

			if(length - offset < this.m_count) {
				throw new ArgumentException($"Invalid offset ({offset}) for source length ({length}).");
			}
		}

		private static int NextPowerOfTwo(int n)
		{
			n--;
			n |= n >> 1;
			n |= n >> 2;
			n |= n >> 4;
			n |= n >> 8;
			n |= n >> 16;
			n++;

			return n;
		}

		private void SetCapacity(int newCapacity)
		{
			newCapacity = NextPowerOfTwo(newCapacity);

			if(newCapacity < this.m_count) {
				throw new ArgumentOutOfRangeException(nameof(newCapacity),
													  "New capacity cannot be less than current collection size.");
			}

			if(newCapacity <= 0) {
				throw new ArgumentOutOfRangeException(nameof(newCapacity), "Cannot set capacity to 0.");
			}

			var array = new TValue[newCapacity];

			if(this.m_data != null) {
				this.CopyToArray(array);
			}

			this.m_data = array;
			this.m_offset = 0;
		}

		private void EnsureCapacity(int count)
		{
			var newCount = this.m_count + count;

			if(newCount <= this.m_data.Length) {
				return;
			}

			this.SetCapacity(newCount);
		}

		private bool IsSplit()
		{
			return this.m_offset > this.m_data.Length - this.m_count;
		}

		private void CheckDisposed()
		{
			if(this.m_isDisposed > 1) {
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		private int FindSmallestCapacity()
		{
			double count = this.m_count;
			var n = Math.Log(this.m_data.Length);
			double newCapacity = this.m_data.Length;

			do {
				var tmp = Math.Pow(2.0D, n);

				if(tmp * 0.9D < count) {
					var rv = newCapacity + double.Epsilon;
					return Convert.ToInt32(rv);
				}

				newCapacity = tmp;
				n -= 1.0D;
			} while(n > 0);


			var result = newCapacity + double.Epsilon;
			return Convert.ToInt32(result);
		}

		protected virtual void Dispose(bool disposing)
		{
			if(Interlocked.Increment(ref this.m_isDisposed) > 1) {
				return;
			}

			if(disposing) {
				this.Clear();
			}

			this.m_data = null;
		}

		private void DoAddRange(IEnumerable<TValue> items)
		{
			var result = CollectionExtensions.ToReadOnlyCollection(items);

			this.EnsureCapacity(result.Count);
			this.CopyFrom(result, this.m_count);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
