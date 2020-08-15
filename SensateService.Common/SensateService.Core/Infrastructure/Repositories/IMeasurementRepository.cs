/*
 * Abstract measurement repository
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;

using SensateService.Enums;
using SensateService.Models;
using SensateService.Models.Generic;

namespace SensateService.Infrastructure.Repositories
{
	using MeasurementMap = IDictionary<ObjectId, List<Measurement>>;

	public interface IMeasurementRepository
	{
		Task<IEnumerable<Measurement>> GetMeasurementsBySensorAsync(Sensor sensor, int skip = -1, int limit = -1);
		Task<IEnumerable<MeasurementsQueryResult>> GetBetweenAsync(Sensor sensor, DateTime start, DateTime end,
																   int skip = -1, int limit = -1,
																   OrderDirection order = OrderDirection.None);
		Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsBetweenAsync(IEnumerable<Sensor> sensors,
																			   DateTime start, DateTime end,
																			   int skip = -1, int limit = -1,
																			   OrderDirection order = OrderDirection.None,
																			   CancellationToken ct = default);

		Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsNearAsync(Sensor sensor, DateTime start, DateTime end, GeoJson2DGeographicCoordinates coords,
			int max = 100, int skip = -1, int limit = -1, OrderDirection order = OrderDirection.None, CancellationToken ct = default);

		Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsNearAsync(IEnumerable<Sensor> sensors, DateTime start, DateTime end, GeoJson2DGeographicCoordinates coords,
			int max = 100, int skip = -1, int limit = -1, OrderDirection order = OrderDirection.None, CancellationToken ct = default);

		Task DeleteBySensorAsync(Sensor sensor, CancellationToken ct = default);
		Task DeleteBetweenAsync(Sensor sensor, DateTime start, DateTime end, CancellationToken ct = default);

		Task<SingleMeasurement> GetMeasurementAsync(MeasurementIndex index, CancellationToken ct = default);

		Task StoreAsync(MeasurementMap measurements, CancellationToken ct = default);
		Task StoreAsync(Sensor sensor, Measurement measurement, CancellationToken ct = default);
	}
}
