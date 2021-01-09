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

using MongoDB.Bson;

using SensateService.Common.Data.Enums;
using SensateService.Common.Data.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface ISensorStatisticsRepository
	{
		Task<IEnumerable<SensorStatisticsEntry>> GetAfterAsync(IEnumerable<Sensor> sensors, DateTime dt);
		Task<IEnumerable<SensorStatisticsEntry>> GetAfterAsync(DateTime date);
		Task<IEnumerable<SensorStatisticsEntry>> GetBetweenAsync(Sensor sensor, DateTime start, DateTime end);
		Task<IEnumerable<SensorStatisticsEntry>> GetBetweenAsync(IList<Sensor> sensors, DateTime start, DateTime end);
		Task<IEnumerable<SensorStatisticsEntry>> GetAsync(Expression<Func<SensorStatisticsEntry, bool>> expr);
	}
}