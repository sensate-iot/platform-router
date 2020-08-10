/*
 * List partitioning extension method.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;

namespace SensateService.Helpers
{
	public static class PartitionListHelper
	{
		public static IEnumerable<IList<T>> Partition<T>(
			this IEnumerable<T> source,
			int size
		)
		{
			if(size < 2) {
				throw new ArgumentOutOfRangeException(
					nameof(size),
					size,
					"Must be greater or equal to 2."
				);
			}

			T[] partition;
			int count;

			using(var e = source.GetEnumerator()) {
				if(e.MoveNext()) {
					partition = new T[size];
					partition[0] = e.Current;
					count = 1;
				} else {
					yield break;
				}

				while(e.MoveNext()) {
					partition[count] = e.Current;
					count++;

					if(count != size) {
						continue;
					}

					yield return partition;
					count = 0;
					partition = new T[size];
				}
			}

			if(count <= 0) {
				yield break;
			}

			Array.Resize(ref partition, count);
			yield return partition;
		}
	}
}