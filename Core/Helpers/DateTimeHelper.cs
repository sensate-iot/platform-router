/*
 * DateTime helper methods.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
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

		public static DateTime ThisHour(this DateTime dt)
		{
			DateTime rounded;

			rounded = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, dt.Kind);
			return rounded;
		}
	}
}
