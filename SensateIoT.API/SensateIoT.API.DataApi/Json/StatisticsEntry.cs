/*
 * Sensor data statistics entry.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using SensateIoT.API.Common.Data.Models;

namespace SensateIoT.API.DataApi.Json
{
	public class StatisticsEntry
	{
		public string SensorId { get; set; }
		public IEnumerable<SensorStatisticsEntry> Statistics { get; set; }
	}
}