/*
 * Helper methods for IEnumerable.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;

namespace SensateService.Helpers
{
	public static class IEnumerableHelper
	{
		public static IEnumerable<T> DistinctBy<T, V>(this IEnumerable<T> source, Func<T, V> selector)
		{
			var seen = new HashSet<V>();

			foreach(var element in source) {
				if(seen.Add(selector(element))) {
					yield return element;
				}
			}
		}
	}
}