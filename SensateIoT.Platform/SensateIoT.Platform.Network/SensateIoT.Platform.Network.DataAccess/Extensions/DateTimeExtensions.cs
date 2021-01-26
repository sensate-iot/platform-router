/*
 * DateTime extensions.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;

namespace SensateIoT.Platform.Network.DataAccess.Extensions
{
	public static class DateTimeExtensions
	{
		public static DateTime ThisHour(this DateTime dt)
		{
			DateTime rounded;

			rounded = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, dt.Kind);
			return rounded;
		}
	}
}
