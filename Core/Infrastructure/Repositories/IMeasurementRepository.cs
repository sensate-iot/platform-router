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
using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using SensateService.Models;
using SensateService.Models.Generic;

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

		Task<IEnumerable<MeasurementsGeoQueryResult>> GetMeasurementsNearAsync(Sensor sensor, DateTime start, DateTime end, GeoJson2DGeographicCoordinates coords, int skip = -1, int limit = -1, CancellationToken ct = default);

		Task DeleteBySensorAsync(Sensor sensor);
		Task DeleteBetweenAsync(Sensor sensor, DateTime start, DateTime end);
		Task DeleteAsync(string id);

		Task<SingleMeasurement> GetMeasurementAsync(MeasurementIndex index, CancellationToken ct = default);
		Task<MeasurementIndex> GetMeasurementIndexAsync(ObjectId sensorId, DateTime timestamp, CancellationToken ct = default);

		Task StoreAsync(MeasurementMap measurements, CancellationToken ct = default(CancellationToken));
		Task StoreAsync(Sensor sensor, Measurement measurement, CancellationToken ct = default(CancellationToken));
	}
}
