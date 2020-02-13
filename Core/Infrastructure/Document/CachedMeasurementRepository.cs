/*
 * Cached measurement repository
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateService.Helpers;
using SensateService.Infrastructure.Cache;
using SensateService.Models;
using SensateService.Models.Generic;

namespace SensateService.Infrastructure.Document
{
	public class CachedMeasurementRepository : MeasurementRepository
	{
		private readonly ICacheStrategy<string> _cache;

		public CachedMeasurementRepository(
			SensateContext context,
			ILogger<MeasurementRepository> logger,
			ICacheStrategy<string> cache) : base(context, logger)
		{
			_cache = cache;
		}

		public override async Task DeleteAsync(string id)
		{
			var tasks = new[] {
				_cache.RemoveAsync(id),
				base.DeleteAsync(id)
			};

			await Task.WhenAll(tasks).AwaitBackground();
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
			return await TryGetMeasurementsAsync(key, x => x.SensorId == sensor.InternalId, CacheTimeout.TimeoutMedium.ToInt(),
				skip, limit ).AwaitBackground();
		}

		private async Task<IEnumerable<Measurement>> TryGetMeasurementsAsync(string key, Expression<Func<MeasurementBucket, bool>> expression, int tmo, int skip, int limit)
		{
			IEnumerable<Measurement> measurements = null;

			if(key != null)
				measurements = await _cache.DeserializeAsync<IEnumerable<Measurement>>(key);

			if(measurements != null)
				return measurements;

			measurements = await base.GetMeasurementsAsync(expression, skip, limit).AwaitBackground();
			await this.CacheDataAsync(key, measurements, tmo, false).AwaitBackground();
			return measurements;

		}

		public override async Task<IEnumerable<MeasurementsQueryResult>> GetAfterAsync(Sensor sensor, DateTime pit, int skip = -1, int limit = -1)
		{
			string key;
			IEnumerable<MeasurementsQueryResult> measurements;

			key = $"{sensor.InternalId}::after::{pit.ToString(CultureInfo.InvariantCulture)}::{skip}::{limit}";
			measurements = await _cache.DeserializeAsync<IEnumerable<MeasurementsQueryResult>>(key);

			if(measurements != null)
				return measurements;

			measurements = await base.GetAfterAsync(sensor, pit, skip, limit).AwaitBackground();
			await this.CacheDataAsync(key, measurements, CacheTimeout.TimeoutMedium.ToInt(), false).AwaitBackground();
			return measurements;

		}

		public override async Task<IEnumerable<MeasurementsQueryResult>> GetBetweenAsync(Sensor sensor, DateTime start, DateTime end, int skip = -1, int limit = -1)
		{
			string key;
			IEnumerable<MeasurementsQueryResult> measurements;

			key = $"{sensor.InternalId}::{start.ToString(CultureInfo.InvariantCulture)}::{end.ToString(CultureInfo.InvariantCulture)}::{skip}::{limit}";
			measurements = await _cache.DeserializeAsync<IEnumerable<MeasurementsQueryResult>>(key);

			if(measurements != null)
				return measurements;

			measurements = await base.GetBetweenAsync(sensor, start, end, skip, limit).AwaitBackground();
			await CacheDataAsync(key, measurements, CacheTimeout.Timeout.ToInt()).AwaitBackground();
			return measurements;


		}

		public override async Task<IEnumerable<MeasurementsQueryResult>> GetBeforeAsync(Sensor sensor, DateTime pit, int skip = -1, int limit = -1)
		{
			string key;
			IEnumerable<MeasurementsQueryResult> measurements;

			key = $"{sensor.InternalId}::before::{pit.ToString(CultureInfo.InvariantCulture)}::{skip}::{limit}";
			measurements = await this._cache.DeserializeAsync<IEnumerable<MeasurementsQueryResult>>(key).AwaitBackground();

			if(measurements != null)
				return null;

			measurements = await base.GetBeforeAsync(sensor, pit, skip, limit).AwaitBackground();
			await this.CacheDataAsync(key, measurements, CacheTimeout.Timeout.ToInt()).AwaitBackground();
			return measurements;
		}
	}
}
