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

using SensateService.Infrastructure.Events;
using SensateService.Models;
using SensateService.Models.Json.In;

namespace SensateService.Infrastructure.Repositories
{
	public interface IMeasurementRepository
	{
		event OnMeasurementReceived MeasurementReceived;

		IEnumerable<Measurement> TryGetBetween(Sensor sensor, DateTime start, DateTime end);
		IEnumerable<Measurement> TryGetMeasurements(Expression<Func<Measurement, bool>> selector);
		Measurement GetMeasurement(Expression<Func<Measurement, bool>> selector);
		IEnumerable<Measurement> GetMeasurementsBySensor(Sensor sensor);
		IEnumerable<Measurement> GetBefore(Sensor sensor, DateTime pit);
		IEnumerable<Measurement> GetAfter(Sensor sensor, DateTime pit);
		Task<long> GetMeasurementCountAsync(Sensor sensor, CancellationToken token = default(CancellationToken));
		Task<Measurement> GetByIdAsync(string id);

		Task<IEnumerable<Measurement>> TryGetBetweenAsync(Sensor sensor, DateTime start, DateTime end);
		Task<IEnumerable<Measurement>> TryGetMeasurementsAsync(Expression<Func<Measurement, bool>> selector);
		Task<Measurement> GetMeasurementAsync(Expression<Func<Measurement, bool>> selector);
		Task<IEnumerable<Measurement>> GetMeasurementsBySensorAsync(Sensor sensor);
		Task<IEnumerable<Measurement>> GetBeforeAsync(Sensor sensor, DateTime pit);
		Task<IEnumerable<Measurement>> GetAfterAsync(Sensor sensor, DateTime pit);

		void DeleteBySensor(Sensor sensor);
		Task DeleteBySensorAsync(Sensor sensor);
		void DeleteBetween(Sensor sensor, DateTime start, DateTime end);
		Task DeleteBetweenAsync(Sensor sensor, DateTime start, DateTime end);
		Task UpdateAsync(Measurement obj);

		Task ReceiveMeasurementAsync(Sensor sensor, RawMeasurement measurement);
		Task CreateAsync(Measurement obj, CancellationToken ct = default(CancellationToken));
	}
}
