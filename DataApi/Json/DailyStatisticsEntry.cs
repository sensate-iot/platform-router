/*
 * Daily statistics entry.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateService.DataApi.Json
{
	public class DailyStatisticsEntry
	{
		public int DayOfTheWeek { get; set; }
		public long Measurements { get; set; }
	}
}
