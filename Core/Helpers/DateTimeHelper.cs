/*
 * DateTime helper methods.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;

namespace SensateService.Helpers
{
	public static class DateTimeHelper
	{
		public static bool IsNever(this DateTime time)
		{
			return time.CompareTo(DateTime.MinValue) <= 0;
		}

		public static bool IsNever(this DateTime? time)
		{
			if(!time.HasValue)
				return false;

			return time.Value.CompareTo(DateTime.MinValue) <= 0;
		}
	}
}
