/*
 * Sensor statistics DI interface.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
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
		Task<IEnumerable<SensorStatisticsEntry>> GetAfterAsync(IEnumerable<Sensor> sensors, DateTime dt);
		Task<IEnumerable<SensorStatisticsEntry>> GetAfterAsync(Sensor sensor, DateTime date);
		Task<IEnumerable<SensorStatisticsEntry>> GetAfterAsync(DateTime date);
		Task<IEnumerable<SensorStatisticsEntry>> GetBetweenAsync(Sensor sensor, DateTime start, DateTime end);
		Task<IEnumerable<SensorStatisticsEntry>> GetBetweenAsync(IList<Sensor> sensors, DateTime start, DateTime end);
		Task<IEnumerable<SensorStatisticsEntry>> GetAsync(Expression<Func<SensorStatisticsEntry, bool>> expr);

		Task DeleteBySensorAsync(Sensor sensor, CancellationToken ct = default);
		Task DeleteBySensorAsync(Sensor sensor, DateTime from, DateTime to, CancellationToken ct = default);
	}
}