/*
 * Abstract measurement repository
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Threading;

using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IMeasurementRepository
	{
		Task<long> GetMeasurementCountAsync(Sensor sensor, CancellationToken token = default(CancellationToken));

		Task<Measurement> GetByIdAsync(string id);
		Task<IEnumerable<Measurement>> GetMeasurementsAsync(Expression<Func<Measurement, bool>> selector);
		Task<Measurement> GetMeasurementAsync(Expression<Func<Measurement, bool>> selector);
		Task<IEnumerable<Measurement>> GetMeasurementsBySensorAsync(Sensor sensor);
		Task<IEnumerable<Measurement>> GetBeforeAsync(Sensor sensor, DateTime pit);
		Task<IEnumerable<Measurement>> GetAfterAsync(Sensor sensor, DateTime pit);
		Task<IEnumerable<Measurement>> GetBetweenAsync(Sensor sensor, DateTime start, DateTime end);

		Task DeleteBySensorAsync(Sensor sensor);
		Task DeleteBetweenAsync(Sensor sensor, DateTime start, DateTime end);
		Task DeleteAsync(string id);

		Task UpdateAsync(Measurement obj);
		Task CreateAsync(Measurement obj, CancellationToken ct = default(CancellationToken));
	}
}
