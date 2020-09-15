/*
 * Geo query service implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Driver.GeoJsonObjectModel;

using SensateService.Common.Data.Dto.Generic;
using SensateService.Common.Data.Enums;

namespace SensateService.Services.DataProcessing
{
	public class GeoQueryService : IGeoQueryService
	{
		private const double EarthRadius = 6371000;
		private const int ErrorMarginBoundary = 3000;
		private const int MeasurementMargin = 10000;

		private static double DegToRad(double deg)
		{
			return deg * (Math.PI / 180);
		}

		private static double CalculateDistanceBetween(GeoJson2DGeographicCoordinates p1, GeoJson2DGeographicCoordinates p2)
		{
			var dLat = DegToRad(p2.Latitude - p1.Latitude);
			var dLng = DegToRad(p2.Longitude - p1.Longitude);
			var x =
				Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
				Math.Cos(DegToRad(p1.Latitude)) * Math.Cos(DegToRad(p2.Latitude)) *
				Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
			var arc = 2 * Math.Atan2(Math.Sqrt(x), Math.Sqrt(1 - x));

			return EarthRadius * arc;
		}

		private static double FastCalculateDistanceBetween(GeoJson2DGeographicCoordinates p1, GeoJson2DGeographicCoordinates p2)
		{
			var lat1 = DegToRad(p1.Latitude);
			var lat2 = DegToRad(p2.Latitude);

			var lng1 = DegToRad(p1.Longitude);
			var lng2 = DegToRad(p2.Longitude);

			var x = (lng2 - lng1) * Math.Cos((lat1 + lat2) / 2);
			var y = lat2 - lat1;

			return Math.Sqrt(x * x + y * y) * EarthRadius;
		}

		private delegate double DistanceCalcuationMethod(GeoJson2DGeographicCoordinates p1, GeoJson2DGeographicCoordinates p2);

		public IList<MeasurementsQueryResult> GetMeasurementsNear(
			List<MeasurementsQueryResult> measurements, GeoJson2DGeographicCoordinates coords,
			int radius = 100, int skip = -1, int limit = -1, OrderDirection order = OrderDirection.None, CancellationToken ct = default)
		{
			DistanceCalcuationMethod calc;
			var queryResults = new List<MeasurementsQueryResult>(measurements.Count);

			for(var idx = 0; idx < measurements.Count; idx++) {
				queryResults.Add(null);
			}

			var results = queryResults;

			if(radius >= ErrorMarginBoundary || measurements.Count > MeasurementMargin) {
				calc = FastCalculateDistanceBetween;
			} else {
				calc = CalculateDistanceBetween;
			}

			Parallel.For(0, measurements.Count, (index, state) => {
				double maxDist = radius;
				var measurement = measurements[index];
				var dist = calc(coords, measurement.Location.Coordinates);

				if(dist > maxDist) {
					return;
				}

				results[index] = measurement;
			});

			queryResults.RemoveAll(x => x == null);

			if(order == OrderDirection.Descending) {
				queryResults = queryResults.OrderByDescending(x => x.Timestamp).ToList();
			} else if(order == OrderDirection.Ascending) {
				queryResults = queryResults.OrderBy(x => x.Timestamp).ToList();
			}

			if(skip > 0) {
				queryResults = queryResults.Skip(skip).ToList();
			}

			if(limit > 0) {
				queryResults = queryResults.Take(limit).ToList();
			}

			return queryResults;
		}
	}
}