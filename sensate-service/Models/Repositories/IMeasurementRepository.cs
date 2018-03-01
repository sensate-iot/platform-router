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

namespace SensateService.Models.Repositories
{
	public interface IMeasurementRepository
	{
		IEnumerable<Measurement> TryGetBetween(Sensor sensor, DateTime start, DateTime end);
		IEnumerable<Measurement> TryGetMeasurements(string key, Expression<Func<Measurement, bool>> selector);
		Measurement GetMeasurement(string key, Expression<Func<Measurement, bool>> selector);
		IEnumerable<Measurement> GetMeasurementsBySensor(Sensor sensor);

		Task<IEnumerable<Measurement>> TryGetBetweenAsync(Sensor sensor, DateTime start, DateTime end);
		Task<IEnumerable<Measurement>> TryGetMeasurementsAsync(string key, Expression<Func<Measurement, bool>> selector);
		Task<Measurement> GetMeasurementAsync(string key, Expression<Func<Measurement, bool>> selector);
		Task<IEnumerable<Measurement>> GetMeasurementsBySensorAsync(Sensor sensor);
	}
}
