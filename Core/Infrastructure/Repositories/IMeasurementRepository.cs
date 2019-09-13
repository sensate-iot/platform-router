/*
 * Abstract measurement repository
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Threading;

using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	using MeasurementMap = IDictionary<Sensor, List<Measurement>>;

	public interface IMeasurementRepository
	{
		Task<long> GetMeasurementCountAsync(Sensor sensor, CancellationToken token = default(CancellationToken));

		Task<IEnumerable<Measurement>> GetMeasurementsAsync(Expression<Func<MeasurementBucket, bool>> selector, int skip = -1, int limit = -1);
		Task<IEnumerable<Measurement>> GetMeasurementsAsync(Expression<Func<MeasurementBucket, bool>> expr, Func<Measurement, bool> mexpr, int skip = -1, int limit = -1);
		Task<IEnumerable<Measurement>> GetMeasurementsBySensorAsync(Sensor sensor, int skip = -1, int limit = -1);
		Task<IEnumerable<Measurement>> GetBeforeAsync(Sensor sensor, DateTime pit, int skip = -1, int limit = -1);
		Task<IEnumerable<Measurement>> GetAfterAsync(Sensor sensor, DateTime pit, int skip = -1, int limit = -1);
		Task<IEnumerable<Measurement>> GetBetweenAsync(Sensor sensor, DateTime start, DateTime end, int skip = -1, int limit = -1);

		Task DeleteBySensorAsync(Sensor sensor);
		Task DeleteBetweenAsync(Sensor sensor, DateTime start, DateTime end);
		Task DeleteAsync(string id);

		Task StoreAsync(MeasurementMap measurements, CancellationToken ct = default(CancellationToken));
		Task StoreAsync(Sensor sensor, Measurement measurement, CancellationToken ct = default(CancellationToken));
	}
}
