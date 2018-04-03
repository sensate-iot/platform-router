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

using SensateService.Models;
using Newtonsoft.Json.Linq;

namespace SensateService.Infrastructure.Repositories
{
	public interface IMeasurementRepository
	{
		IEnumerable<Measurement> TryGetBetween(Sensor sensor, DateTime start, DateTime end);
		IEnumerable<Measurement> TryGetMeasurements(string key, Expression<Func<Measurement, bool>> selector);
		Measurement GetMeasurement(string key, Expression<Func<Measurement, bool>> selector);
		IEnumerable<Measurement> GetMeasurementsBySensor(Sensor sensor);
		IEnumerable<Measurement> GetBefore(Sensor sensor, DateTime pit);
		IEnumerable<Measurement> GetAfter(Sensor sensor, DateTime pit);

		Task<IEnumerable<Measurement>> TryGetBetweenAsync(Sensor sensor, DateTime start, DateTime end);
		Task<IEnumerable<Measurement>> TryGetMeasurementsAsync(string key, Expression<Func<Measurement, bool>> selector);
		Task<Measurement> GetMeasurementAsync(string key, Expression<Func<Measurement, bool>> selector);
		Task<IEnumerable<Measurement>> GetMeasurementsBySensorAsync(Sensor sensor);
		Task<IEnumerable<Measurement>> GetBeforeAsync(Sensor sensor, DateTime pit);
		Task<IEnumerable<Measurement>> GetAfterAsync(Sensor sensor, DateTime pit);

		void Create(Measurement m);
		Task CreateAsync(Measurement m);
		Task ReceiveMeasurement(Sensor sender, JToken measurement);
	}
}
