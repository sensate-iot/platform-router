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

using SensateService.Helpers;
using SensateService.Infrastructure.Cache;
using SensateService.Models;
using SensateService.Models.Generic;
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

		private async Task CacheDataAsync(string key, object data, int tmo, bool slide = true)
		{
			if(key == null || data == null)
				return;

			await this._cache.SerializeAsync(key, data, tmo, slide).AwaitBackground();
		}

		public override async Task DeleteBySensorAsync(Sensor sensor)
		{
			string key;

			key = $"Measurements::{sensor.InternalId.ToString()}";
			var tasks = new[] {
				this._cache.RemoveAsync(key),
				base.DeleteBySensorAsync(sensor)
			};

			await Task.WhenAll(tasks).AwaitBackground();
		}

		public override async Task DeleteBetweenAsync(Sensor sensor, DateTime start, DateTime end)
		{
			string key;

			key = $"{sensor.InternalId}::{start.ToString(CultureInfo.InvariantCulture)}::{end.ToString(CultureInfo.InvariantCulture)}";
			var tasks = new[] {
				this._cache.RemoveAsync(key),
				base.DeleteBetweenAsync(sensor, start, end)
			};

			await Task.WhenAll(tasks).AwaitBackground();
		}

		public override async Task<IEnumerable<Measurement>> GetMeasurementsBySensorAsync(Sensor sensor, int skip = -1, int limit = -1)
		{
			string key;

			key = $"Measurements::{sensor.InternalId.ToString()}";
			var measurements = await this._cache.DeserializeAsync<IEnumerable<Measurement>>(key).AwaitBackground();

			if(measurements != null) {
				return measurements;
			}

			measurements = await base.GetMeasurementsBySensorAsync(sensor, skip, limit).AwaitBackground();
			await this.CacheDataAsync(key, measurements, CacheTimeout.TimeoutShort.ToInt(), false).AwaitBackground();

			return measurements;
		}

		public override async Task<IEnumerable<MeasurementsQueryResult>> GetBetweenAsync(Sensor sensor, DateTime start, DateTime end, int skip = -1, int limit = -1)
		{
			string key;
			IEnumerable<MeasurementsQueryResult> measurements;

			var cache_end = end.ThisMinute();
			var cache_start = start.ThisMinute();

			key = $"{sensor.InternalId}::{cache_start.ToString("u", CultureInfo.InvariantCulture)}::{cache_end.ToString("u", CultureInfo.InvariantCulture)}::{skip}::{limit}";
			measurements = await _cache.DeserializeAsync<IEnumerable<MeasurementsQueryResult>>(key);

			if(measurements != null)
				return measurements;

			measurements = await base.GetBetweenAsync(sensor, start, end, skip, limit).AwaitBackground();
			await this.CacheDataAsync(key, measurements, CacheTimeout.TimeoutShort.ToInt(), false).AwaitBackground();
			return measurements;


		}

		public override async Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsNearAsync(Sensor sensor,
			DateTime start,
			DateTime end, GeoJson2DGeographicCoordinates coords,
			int max = 100, int skip = -1, int limit = -1, CancellationToken ct = default)
		{
			IEnumerable<MeasurementsQueryResult> measurements;
			var cache_end = end.ThisMinute();
			var cache_start = start.ThisMinute();

			var key =
				$"{sensor.InternalId}::" +
				$"{cache_start.ToString("u", CultureInfo.InvariantCulture)}::" +
				$"{cache_end.ToString("u", CultureInfo.InvariantCulture)}::" +
				$"{skip}::{limit}::" +
				$"{max}::{coords.Longitude}::{coords.Latitude}";
			measurements = await _cache.DeserializeAsync<IEnumerable<MeasurementsQueryResult>>(key, ct);

			if(measurements != null) {
				return measurements;
			}

			measurements = await base.GetMeasurementsNearAsync(sensor, start, end, coords, max, skip, limit, ct).AwaitBackground();
			await this.CacheDataAsync(key, measurements, CacheTimeout.TimeoutShort.ToInt(), false).AwaitBackground();

			return measurements;
		}


		public override async Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsBetweenAsync(
			IEnumerable<Sensor> sensors,
			DateTime start,
			DateTime end,
			int skip = -1,
			int limit = -1,
			CancellationToken ct = default)
		{
			var ordered = sensors.OrderBy(x => x.InternalId).ToList();
			var cache_end = end.ThisMinute();
			var cache_start = start.ThisMinute();

			var key =
				$"Near::{ordered.GetHashCode()}::" +
				$"{cache_start.ToString("u", CultureInfo.InvariantCulture)}::" +
				$"{cache_end.ToString("u", CultureInfo.InvariantCulture)}::" +
				$"{skip}::{limit}::";

			var measurements = await this._cache.DeserializeAsync<IEnumerable<MeasurementsQueryResult>>(key, ct);

			if(measurements != null) {
				return measurements;
			}

			measurements = await base.GetMeasurementsBetweenAsync(ordered, start, end, skip, limit, ct).AwaitBackground();
			await this.CacheDataAsync(key, measurements, CacheTimeout.TimeoutShort.ToInt(), false).AwaitBackground();

			return measurements;
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
			int max = 100, int skip = -1, int limit = -1, CancellationToken ct = default)
		{
			var ordered = sensors.OrderBy(x => x.InternalId).ToList();
			var cache_end = end.ThisMinute();
			var cache_start = start.ThisMinute();

			var key =
				$"Near::{GetSensorListHashCode(ordered)}::" +
				$"{cache_start.ToString("u", CultureInfo.InvariantCulture)}::" +
				$"{cache_end.ToString("u", CultureInfo.InvariantCulture)}::" +
				$"{skip}::{limit}::" +
				$"{max}::{coords.Longitude}::{coords.Latitude}";

			var measurements = await this._cache.DeserializeAsync<IEnumerable<MeasurementsQueryResult>>(key, ct);

			if(measurements != null) {
				return measurements;
			}

			measurements = await base.GetMeasurementsNearAsync(ordered, start, end, coords, max, skip, limit, ct).AwaitBackground();
			await this.CacheDataAsync(key, measurements, CacheTimeout.TimeoutShort.ToInt(), false).AwaitBackground();

			return measurements;
		}
	}
}
