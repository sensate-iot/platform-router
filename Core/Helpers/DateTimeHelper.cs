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
	}
}
