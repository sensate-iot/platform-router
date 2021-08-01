/*
 * Collection extension methods.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace SensateIoT.Platform.Router.Common.Collections.Local
{
	internal static class CollectionExtensions
	{
		internal static IReadOnlyCollection<TValue> ToReadOnlyCollection<TValue>(IEnumerable<TValue> source)
		{
			switch(source) {
			case null:
				throw new ArgumentNullException(nameof(source));
			case IReadOnlyCollection<TValue> result:
				return result;
			case ICollection<TValue> collection:
				return new CollectionWrapper<TValue>(collection);
			case ICollection nongenericCollection:
				return new NongenericCollectionWrapper<TValue>(nongenericCollection);
			default:
				return new List<TValue>(source);
			}
		}

		private sealed class NongenericCollectionWrapper<T> : IReadOnlyCollection<T>
		{
			private readonly ICollection _collection;

			public NongenericCollectionWrapper(ICollection collection)
			{
				this._collection = collection ?? throw new ArgumentNullException(nameof(collection));
			}

			public int Count => this._collection.Count;

			public IEnumerator<T> GetEnumerator()
			{
				foreach(T item in this._collection) {
					yield return item;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this._collection.GetEnumerator();
			}
		}

		private sealed class CollectionWrapper<T> : IReadOnlyCollection<T>
		{
			private readonly ICollection<T> m_collection;

			public CollectionWrapper(ICollection<T> collection)
			{
				this.m_collection = collection ?? throw new ArgumentNullException(nameof(collection));
			}

			public int Count => this.m_collection.Count;

			public IEnumerator<T> GetEnumerator()
			{
				return this.m_collection.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.m_collection.GetEnumerator();
			}
		}
	}
}