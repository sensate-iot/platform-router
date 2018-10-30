/*
 * Cached measurement repository
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateService.Exceptions;
using SensateService.Helpers;
using SensateService.Infrastructure.Cache;
using SensateService.Models;

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

		private void Commit(Measurement obj)
		{
			try {
				_cache.Serialize(obj.InternalId.ToString(), obj, CacheTimeout.Timeout.ToInt(), true);
			} catch(Exception ex) {
				throw new CachingException(
					$"Unable to cache measurement: {ex.Message}",
					obj.InternalId.ToString(), ex
				);
			}
		}

		private async Task CommitAsync(Measurement obj, CancellationToken ct = default(CancellationToken))
		{
			try {
				await _cache.SerializeAsync(obj.InternalId.ToString(), obj, CacheTimeout.Timeout.ToInt(), true, ct).AwaitSafely();
			} catch(Exception ex) {
				throw new CachingException(
					$"Unable to cache measurement: {ex.Message}",
					obj.InternalId.ToString(), ex
				);
			}
		}

		public override void Delete(string id)
		{
			_cache.Remove(id);
			base.Delete(id);
		}

		public override void Create(Measurement m)
		{
			base.Create(m);
			this.Commit(m);
		}

		public override async Task CreateAsync(Measurement obj)
		{
			var tasks = new Task[2];

			tasks[0] = base.CreateAsync(obj);
			tasks[1] = this.CommitAsync(obj);

			await Task.WhenAll(tasks).AwaitSafely();
		}

		public override async Task DeleteAsync(string id)
		{
			var tasks = new[] {
				_cache.RemoveAsync(id),
				base.DeleteAsync(id)
			};

			await Task.WhenAll(tasks).AwaitSafely();
		}

		private void CacheData(string key, object data, int tmo, bool slide = true)
		{
			if(key == null || data == null)
				return;

			this._cache.Serialize(key, data, tmo, slide);
		}

		private async Task CacheDataAsync(string key, object data, int tmo, bool slide = true)
		{
			if(key == null || data == null)
				return;

			await this._cache.SerializeAsync(key, data, tmo, slide).AwaitSafely();
		}

		public override void DeleteBySensor(Sensor sensor)
		{
			string key;

			key = String.Format("Measurements::{0}", sensor.InternalId.ToString());
			_cache.Remove(key);
			base.DeleteBySensor(sensor);
		}

		public override async Task DeleteBySensorAsync(Sensor sensor)
		{
			string key;

			key = String.Format("Measurements::{0}", sensor.InternalId.ToString());
			var tasks = new[] {
				this._cache.RemoveAsync(key),
				base.DeleteBySensorAsync(sensor)
			};

			await Task.WhenAll(tasks).AwaitSafely();
		}

		public override void DeleteBetween(Sensor sensor, DateTime start, DateTime end)
		{
			string key;

			key = $"{sensor.InternalId}::{start.ToString(CultureInfo.InvariantCulture)}::{end.ToString(CultureInfo.InvariantCulture)}";
			_cache.Remove(key);
			base.DeleteBetween(sensor, start, end);
		}

		public override async Task DeleteBetweenAsync(Sensor sensor, DateTime start, DateTime end)
		{
			string key;

			key = $"{sensor.InternalId}::{start.ToString(CultureInfo.InvariantCulture)}::{end.ToString(CultureInfo.InvariantCulture)}";
			var tasks = new[] {
                this._cache.RemoveAsync(key),
                base.DeleteBetweenAsync(sensor, start, end)
			};

			await Task.WhenAll(tasks).AwaitSafely();
		}

		public override Measurement GetById(string id)
		{
			Measurement m;

			m = _cache.Deserialize<Measurement>(id);

			if(m != null)
				return m;

			m = base.GetById(id);

			if(m != null)
				this.Commit(m);

			return m;
		}

		public override async Task<IEnumerable<Measurement>> GetMeasurementsBySensorAsync(Sensor sensor)
		{
			string key;

			key = String.Format("Measurements::{0}", sensor.InternalId.ToString());
			return await TryGetMeasurementsAsync(key,
				x => x.CreatedBy == sensor.InternalId, CacheTimeout.TimeoutMedium.ToInt()
			).AwaitSafely();
		}

		public override IEnumerable<Measurement> GetMeasurementsBySensor(Sensor sensor)
		{
			string key;

			key = String.Format("Measurements::{0}", sensor.InternalId.ToString());
			return TryGetMeasurements(key, x =>
				x.CreatedBy == sensor.InternalId, CacheTimeout.TimeoutMedium.ToInt()
			);
		}

		public override void Update(Measurement obj)
		{
			_cache.Serialize(obj.InternalId.ToString(), obj, CacheTimeout.Timeout.ToInt(), false);
			base.Update(obj);
		}

		private async Task<IEnumerable<Measurement>> TryGetMeasurementsAsync(
			string key, Expression<Func<Measurement, bool>> expression, int tmo
		)
		{
			IEnumerable<Measurement> measurements = null;

			if(key != null)
				measurements = await _cache.DeserializeAsync<IEnumerable<Measurement>>(key);

			if(measurements != null)
				return measurements;

			measurements = await base.TryGetMeasurementsAsync(expression).AwaitSafely();
			await this.CacheDataAsync(key, measurements, tmo, false).AwaitSafely();
			return measurements;

		}

		private IEnumerable<Measurement> TryGetMeasurements(string key, Expression<Func<Measurement, bool>> selector, int tmo)
		{
			IEnumerable<Measurement> measurements = null;

			if(key != null)
				measurements = _cache.Deserialize<IEnumerable<Measurement>>(key);

			if(measurements != null)
				return measurements;

			measurements = base.TryGetMeasurements(selector);
			this.CacheData(key, measurements, tmo, false);
			return measurements;
		}

		public override async Task<IEnumerable<Measurement>> GetAfterAsync(Sensor sensor, DateTime pit)
		{
			string key;
			IEnumerable<Measurement> measurements;

			key = $"{sensor.InternalId}::after::{pit.ToString(CultureInfo.InvariantCulture)}";
			measurements = await _cache.DeserializeAsync<IEnumerable<Measurement>>(key);

			if(measurements != null)
				return measurements;

			measurements = await base.GetAfterAsync(sensor, pit).AwaitSafely();
			await this.CacheDataAsync(key, measurements, CacheTimeout.TimeoutMedium.ToInt(), false).AwaitSafely();
			return measurements;

		}

		public override IEnumerable<Measurement> GetAfter(Sensor sensor, DateTime pit)
		{
			string key;
			IEnumerable<Measurement> measurements;

			key = $"{sensor.InternalId}::after::{pit.ToString(CultureInfo.InvariantCulture)}";
			measurements = _cache.Deserialize<IEnumerable<Measurement>>(key);

			if(measurements != null)
				return measurements;

			measurements = base.GetAfter(sensor, pit);
			CacheData(key, measurements, CacheTimeout.TimeoutMedium.ToInt(), false);
			return measurements;

		}

		public override IEnumerable<Measurement> TryGetBetween(Sensor sensor, DateTime start, DateTime end)
		{
			string key;
			IEnumerable<Measurement> measurements;

			key = $"{sensor.InternalId}::{start.ToString(CultureInfo.InvariantCulture)}::{end.ToString(CultureInfo.InvariantCulture)}";

			measurements = _cache.Deserialize<IEnumerable<Measurement>>(key);

			if(measurements != null)
				return measurements;

			measurements = base.TryGetBetween(sensor, start, end);
			CacheData(key, measurements, CacheTimeout.Timeout.ToInt());
			return measurements;

		}

		public override async Task<IEnumerable<Measurement>> TryGetBetweenAsync(Sensor sensor, DateTime start, DateTime end)
		{
			string key;
			IEnumerable<Measurement> measurements;

			key = $"{sensor.InternalId}::{start.ToString(CultureInfo.InvariantCulture)}::{end.ToString(CultureInfo.InvariantCulture)}";
			measurements = await _cache.DeserializeAsync<IEnumerable<Measurement>>(key);

			if(measurements != null)
				return measurements;

			measurements = await base.TryGetBetweenAsync(sensor, start, end).AwaitSafely();
			await CacheDataAsync(key, measurements, CacheTimeout.Timeout.ToInt()).AwaitSafely();
			return measurements;


		}

		public override IEnumerable<Measurement> GetBefore(Sensor sensor, DateTime pit)
		{
			string key;
			IEnumerable<Measurement> measurements;

			key = $"{sensor.InternalId}::before::{pit.ToString(CultureInfo.InvariantCulture)}";
			measurements = this._cache.Deserialize<IEnumerable<Measurement>>(key);

			if(measurements != null)
				return null;

			measurements = base.GetBefore(sensor, pit);
			this.CacheData(key, measurements, CacheTimeout.Timeout.ToInt());
			return measurements;
		}

		public override async Task<IEnumerable<Measurement>> GetBeforeAsync(Sensor sensor, DateTime pit)
		{
			string key;
			IEnumerable<Measurement> measurements;

			key = $"{sensor.InternalId}::before::{pit.ToString(CultureInfo.InvariantCulture)}";
			measurements = await this._cache.DeserializeAsync<IEnumerable<Measurement>>(key).AwaitSafely();

			if(measurements != null)
				return null;

			measurements = await base.GetBeforeAsync(sensor, pit).AwaitSafely();
			await this.CacheDataAsync(key, measurements, CacheTimeout.Timeout.ToInt()).AwaitSafely();
			return measurements;
		}
	}
}
