/*
 * Sensor statistics DI interface.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using SensateService.Enums;
using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface ISensorStatisticsRepository
	{
		Task IncrementAsync(Sensor sensor, RequestMethod method);
		Task<SensorStatisticsEntry> CreateForAsync(Sensor sensor);
		Task IncrementManyAsync(Sensor sensor, RequestMethod method, int num, CancellationToken token = default(CancellationToken));

		Task<SensorStatisticsEntry> GetByDateAsync(Sensor sensor, DateTime date);
		Task<IEnumerable<SensorStatisticsEntry>> GetBeforeAsync(Sensor sensor, DateTime date);
		Task<IEnumerable<SensorStatisticsEntry>> GetAfterAsync(Sensor sensor, DateTime date);
		Task<IEnumerable<SensorStatisticsEntry>> GetAfterAsync(DateTime date);
		Task<IEnumerable<SensorStatisticsEntry>> GetBetweenAsync(Sensor sensor, DateTime start, DateTime end);
		Task<IEnumerable<SensorStatisticsEntry>> GetBetweenAsync(IEnumerable<Sensor> sensors, DateTime start, DateTime end);
		Task<IEnumerable<SensorStatisticsEntry>> GetAsync(Expression<Func<SensorStatisticsEntry, bool>> expr);

		void Delete(string id);
		Task DeleteAsync(string id);
		Task DeleteBySensorAsync(Sensor sensor);
		Task DeleteBySensorAsync(Sensor sensor, DateTime from, DateTime to);
	}
}