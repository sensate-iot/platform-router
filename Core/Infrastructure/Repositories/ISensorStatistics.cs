/*
 * Sensor statistics DI interface.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface ISensorStatistics
	{
		Task IncrementAsync(Sensor sensor);
		Task<SensorStatisticsEntry> CreateForAsync(Sensor sensor);

		Task<SensorStatisticsEntry> GetByDateAsync(Sensor sensor, DateTime date);
		Task<IEnumerable<SensorStatisticsEntry>> GetBeforeAsync(Sensor sensor, DateTime date);
		Task<IEnumerable<SensorStatisticsEntry>> GetAfterAsync(Sensor sensor, DateTime date);

		void Delete(string id);
		Task DeleteAsync(string id);
		Task DeleteBySensorAsync(Sensor sensor);
	}
}