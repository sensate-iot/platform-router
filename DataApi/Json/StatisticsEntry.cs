/*
 * Sensor data statistics entry.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;

namespace SensateService.DataApi.Json
{
	public class StatisticsEntry
	{
		public string SensorId { get; set; }
		public IEnumerable<SensateService.Models.SensorStatisticsEntry> Statistics { get; set; }
	}
}