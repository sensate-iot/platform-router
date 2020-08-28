/*
 * Cached measurement repository
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json;
using SensateService.Common.Data.Dto.Generic;
using SensateService.Common.Data.Enums;
using SensateService.Common.Data.Models;
using SensateService.Helpers;
using SensateService.Infrastructure.Cache;
using SensateService.Services;

namespace SensateService.Infrastructure.Document
{
	public class CachedMeasurementRepository : MeasurementRepository
	{
		private readonly ICacheStrategy<string> _cache;

		public CachedMeasurementRepository(
			SensateContext context,
			IGeoQueryService geo,
			ILogger<MeasurementRepository> logger,
			ICacheStrategy<string> cache) : base(context, geo, logger)
		{
			this._cache = cache;
		}

		private Task CacheRangeAsync(string key, IEnumerable<MeasurementsQueryResult> data, int tmo)
		{
			if(key == null || data == null) {
				return Task.CompletedTask;
			}

			var stringified = JsonConvert.SerializeObject(data);
			return this._cache.SetAsync(key, stringified, tmo);
		}

		private async Task<IEnumerable<MeasurementsQueryResult>> GetRangeAsync(string key, CancellationToken ct = default)
		{
			if(key == null) {
				return null;
			}

			var data = await this._cache.GetAsync(key, ct).AwaitBackground();

			return data == null ? null : JsonConvert.DeserializeObject<IEnumerable<MeasurementsQueryResult>>(data);
		}

		public override async Task DeleteBySensorAsync(Sensor sensor, CancellationToken ct = default)
		{
			string key;

			key = $"measurements::{sensor.InternalId}";
			var tasks = new[] {
				this._cache.RemoveAsync(key),
				base.DeleteBySensorAsync(sensor, ct)
			};

			await Task.WhenAll(tasks).AwaitBackground();
		}

		public override async Task DeleteBetweenAsync(Sensor sensor, DateTime start, DateTime end, CancellationToken ct = default)
		{
			string key;

			key = $"{sensor.InternalId}::{start.ToString(CultureInfo.InvariantCulture)}::{end.ToString(CultureInfo.InvariantCulture)}";
			var tasks = new[] {
				this._cache.RemoveAsync(key),
				base.DeleteBetweenAsync(sensor, start, end, ct)
			};

			await Task.WhenAll(tasks).AwaitBackground();
		}

		public override async Task<IEnumerable<MeasurementsQueryResult>> GetBetweenAsync(Sensor sensor,
																						 DateTime start,
																						 DateTime end,
																						 int skip = -1,
																						 int limit = -1,
																						 OrderDirection order = OrderDirection.None)
		{
			string key;
			IList<MeasurementsQueryResult> measurements;

			var cache_end = end.ThisMinute();
			var cache_start = start.ThisMinute();

			key = $"{sensor.InternalId}::{cache_start.ToString("u", CultureInfo.InvariantCulture)}::{cache_end.ToString("u", CultureInfo.InvariantCulture)}::{skip}::{limit}::{order}";
			var tmp = await this.GetRangeAsync(key).ConfigureAwait(false);

			if(tmp != null) {
				return tmp;
			}

			tmp = await base.GetBetweenAsync(sensor, start, end, skip, limit, order).AwaitBackground();
			measurements = tmp.ToList();
			await this.CacheRangeAsync(key, measurements, CacheTimeout.TimeoutShort.ToInt()).AwaitBackground();
			return measurements;


		}

		public override async Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsNearAsync(Sensor sensor,
			DateTime start,
			DateTime end, GeoJson2DGeographicCoordinates coords,
			int max = 100, int skip = -1, int limit = -1, OrderDirection order = OrderDirection.None,
			CancellationToken ct = default)
		{
			IEnumerable<MeasurementsQueryResult> measurements;
			var cache_end = end.ThisMinute();
			var cache_start = start.ThisMinute();

			var key =
				$"{sensor.InternalId}::" +
				$"{cache_start.ToString("u", CultureInfo.InvariantCulture)}::" +
				$"{cache_end.ToString("u", CultureInfo.InvariantCulture)}::" +
				$"{skip}::{limit}::" +
				$"{max}::{coords.Longitude}::{coords.Latitude}::" +
				$"{order}";
			measurements = await this.GetRangeAsync(key, ct).AwaitBackground();

			if(measurements != null) {
				return measurements;
			}

			measurements = await base.GetMeasurementsNearAsync(sensor, start, end, coords, max, skip, limit, order, ct).AwaitBackground();
			var list = measurements.ToList();
			await this.CacheRangeAsync(key, list, CacheTimeout.TimeoutShort.ToInt()).AwaitBackground();

			return list;
		}


		public override async Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsBetweenAsync(
			IEnumerable<Sensor> sensors,
			DateTime start,
			DateTime end,
			int skip = -1,
			int limit = -1,
			OrderDirection order = OrderDirection.None,
			CancellationToken ct = default)
		{
			var ordered = sensors.OrderBy(x => x.InternalId).ToList();
			var cache_end = end.ThisMinute();
			var cache_start = start.ThisMinute();

			var key =
				$"near::{ordered.GetHashCode()}::" +
				$"{cache_start.ToString("u", CultureInfo.InvariantCulture)}::" +
				$"{cache_end.ToString("u", CultureInfo.InvariantCulture)}::" +
				$"{skip}::{limit}::{order}";

			var measurements = await this.GetRangeAsync(key, ct).AwaitBackground();

			if(measurements != null) {
				return measurements;
			}

			measurements = await base.GetMeasurementsBetweenAsync(ordered, start, end, skip, limit, order, ct).AwaitBackground();
			var list = measurements.ToList();
			await this.CacheRangeAsync(key, list, CacheTimeout.TimeoutShort.ToInt()).AwaitBackground();

			return list;
		}

		private static int GetSensorListHashCode(List<Sensor> sensors)
		{
			const int seed = 0x2D2816FE;
			const int prime = 397;

			if(sensors.Count <= 0) {
				return 0;
			}

			return sensors.Aggregate(seed, (current, item) => (current * prime) + item.InternalId.ToString().GetHashCode());
		}

		public override async Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsNearAsync(IEnumerable<Sensor> sensors,
			DateTime start, DateTime end, GeoJson2DGeographicCoordinates coords,
			int max = 100, int skip = -1, int limit = -1,
			OrderDirection order = OrderDirection.None,
			CancellationToken ct = default)
		{
			var ordered = sensors.OrderBy(x => x.InternalId).ToList();
			var cache_end = end.ThisMinute();
			var cache_start = start.ThisMinute();

			var key =
				$"near::{GetSensorListHashCode(ordered)}::" +
				$"{cache_start.ToString("u", CultureInfo.InvariantCulture)}::" +
				$"{cache_end.ToString("u", CultureInfo.InvariantCulture)}::" +
				$"{skip}::{limit}::" +
				$"{max}::{coords.Longitude}::{coords.Latitude}::" +
				$"{order}";

			var measurements = await this.GetRangeAsync(key, ct).AwaitBackground();

			if(measurements != null) {
				return measurements;
			}

			measurements = await base.GetMeasurementsNearAsync(ordered, start, end,
															   coords, max, skip, limit,
															   order, ct).AwaitBackground();
			var list = measurements.ToList();
			await this.CacheRangeAsync(key, list, CacheTimeout.TimeoutShort.ToInt()).AwaitBackground();
			return list;
		}
	}
}
