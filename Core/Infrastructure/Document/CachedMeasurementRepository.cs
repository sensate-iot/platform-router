/*
 * Cached measurement repository
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using SensateService.Infrastructure.Cache;
using SensateService.Models;
using SensateService.Exceptions;
using SensateService.Helpers;

namespace SensateService.Infrastructure.Document
{
	public class CachedMeasurementRepository : MeasurementRepository
	{
		private ICacheStrategy<string> _cache;

		public CachedMeasurementRepository(
			SensateContext context,
			ILogger<MeasurementRepository> logger,
			ICacheStrategy<string> cache) : base(context, logger)
		{
			this._cache = cache;
		}

		public override void Commit(Measurement obj)
		{
			try {
				this._cache.Set(obj.InternalId.ToString(), obj.ToJson());
			} catch(Exception ex) {
				this._logger.LogWarning($"Unable to cache measurement {ex.Message}");
				throw new CachingException(
					$"Unable to cache measurement: {ex.Message}",
					obj.InternalId.ToString(), ex
				);
			}
		}

		public override async Task CommitAsync(Measurement obj)
		{
			try {
				await _cache.SetAsync(
					obj.InternalId.ToString(),
					obj.ToJson(),
					CacheTimeout.Timeout.ToInt(),
					true,
					ct).AwaitSafely();
			} catch(Exception ex) {
				this._logger.LogWarning($"Unable to log measurement {ex.Message}");
				throw new CachingException(
					$"Unable to cache measurement: {ex.Message}",
					obj.InternalId.ToString(), ex
				);
			}
		}

		public override void Delete(string id)
		{
			this._cache.Remove(id);
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

		protected override async Task<Measurement> CreateAsync(Sensor sensor, RawMeasurement raw, CancellationToken token)
		{
			Measurement measurement;
			Task<Measurement> mWorker;
			Task<string> cacheWorker;
			string key, data;

			key = String.Format("Measurements::{0}", sensor.InternalId.ToString());

			try {
				cacheWorker = this._cache.GetAsync(key, token);
				mWorker = base.CreateAsync(sensor, raw, token);

				data = await cacheWorker.AwaitSafely();
				if(String.IsNullOrEmpty(data))
					return await mWorker.AwaitSafely();

				var measurements = JsonConvert.DeserializeObject<IEnumerable<Measurement>>(data);

				if(measurements == null)
					throw new CachingException("Unable to load cached measurements!", key);

				measurement = await mWorker.AwaitSafely();
				measurements.Append(measurement);
				await this.CacheDataAsync(key, JsonConvert.SerializeObject(measurements),
					CacheTimeout.TimeoutShort.ToInt()).AwaitSafely();
			} catch(CachingException ex) {
				this._logger.LogWarning(ex.Message);
				throw ex;
			} catch(Exception ex) {
				this._logger.LogWarning($"Unable to cache measurement: {ex.Message}");
				throw new CachingException($"Unable to load measurements {ex.Message}", key, ex);
			}

			return measurement;
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
			this.CacheData(key, JsonConvert.SerializeObject(data), tmo, slide);
		}

		private void CacheData(string key, string json, int tmo, bool slide = true)
		{
			if(key == null || json == null)
				return;

			this._cache.Set(key, json, tmo, slide);
		}

		private async Task CacheDataAsync(string key, object data, int tmo, bool slide = true)
		{
			await CacheDataAsync(key, JsonConvert.SerializeObject(data), tmo, slide).AwaitSafely();
		}

		private async Task CacheDataAsync(string key, string json, int tmo, bool slide = true)
		{
			if(key == null || json == null)
				return;

			await _cache.SetAsync(key, json, tmo, slide).AwaitSafely();
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
			string data;

			data = this._cache.Get(id);
			if(data == null)
				return base.GetById(id);

			try {
				m = JsonConvert.DeserializeObject<Measurement>(data);
			} catch(JsonSerializationException) {
				return null;
			}

			return m;
		}

		public override async Task<IEnumerable<Measurement>> GetMeasurementsBySensorAsync(Sensor sensor)
		{
			string key;

			key = String.Format("Measurements::{0}", sensor.InternalId.ToString());
			return await TryGetMeasurementsAsync(key, x =>
				x.CreatedBy == sensor.InternalId, CacheTimeout.Timeout.ToInt()
			).AwaitSafely();
		}

		public override IEnumerable<Measurement> GetMeasurementsBySensor(Sensor sensor)
		{
			string key;

			key = String.Format("Measurements::{0}", sensor.InternalId.ToString());
			return TryGetMeasurements(key, x =>
				x.CreatedBy == sensor.InternalId, CacheTimeout.Timeout.ToInt()
			);
		}

		public override void Update(Measurement obj)
		{
			this._cache.Set(obj.InternalId.ToString(),
				JsonConvert.SerializeObject(obj), CacheTimeout.Timeout.ToInt());
			base.Update(obj);
		}

		private async Task<IEnumerable<Measurement>> TryGetMeasurementsAsync(
			string key, Expression<Func<Measurement, bool>> expression, int tmo)
		{
			string data = null;
			IEnumerable<Measurement> measurements;

			if(key != null)
				data = await _cache.GetAsync(key).AwaitSafely();

			if (data != null)
				return JsonConvert.DeserializeObject<IEnumerable<Measurement>>(data);

			measurements = await base.TryGetMeasurementsAsync(key, expression).AwaitSafely();
			await CacheDataAsync(key, measurements, tmo).AwaitSafely();
			return measurements;
		}

		public async override Task<IEnumerable<Measurement>> TryGetMeasurementsAsync(
			string key, Expression<Func<Measurement, bool>> expression)
		{
			return await TryGetMeasurementsAsync(key, expression, CacheTimeout.Timeout.ToInt()).AwaitSafely();
		}

		public override Measurement TryGetMeasurement(
			string key, Expression<Func<Measurement, bool>> expression)
		{
			string data = null;
			Measurement m;

			if(key != null)
				data = this._cache.Get(key);

			if(data == null) {
				m = base.TryGetMeasurement(key, expression);
				this.CacheData(key, m, CacheTimeout.Timeout.ToInt());
				return m;
			}

			return JsonConvert.DeserializeObject<Measurement>(data);
		}

		public async override Task<Measurement> TryGetMeasurementAsync(
			string key, Expression<Func<Measurement, bool>> expression)
		{
			string data = null;
			Measurement m;

			if(key != null)
				data = await _cache.GetAsync(key).AwaitSafely();

			if (data != null)
				return JsonConvert.DeserializeObject<Measurement>(data);

			m = await base.TryGetMeasurementAsync(key, expression).AwaitSafely();
			await CacheDataAsync(key, m, CacheTimeout.Timeout.ToInt()).AwaitSafely();
			return m;
		}

		private IEnumerable<Measurement> TryGetMeasurements(
			string key, Expression<Func<Measurement, bool>> selector, int tmo
		)
		{
			string data = null;
			IEnumerable<Measurement> measurements;

			if(key != null)
				data = this._cache.Get(key);

			if(data == null) {
				measurements = base.TryGetMeasurements(key, selector);
				this.CacheData(key, measurements, CacheTimeout.Timeout.ToInt());
				return measurements;
			}

			return JsonConvert.DeserializeObject<IEnumerable<Measurement>>(data);
		}

		public override IEnumerable<Measurement> TryGetMeasurements(
			string key, Expression<Func<Measurement, bool>> selector
		)
		{
			return this.TryGetMeasurements(key, selector, CacheTimeout.Timeout.ToInt());
		}

		public override Measurement GetMeasurement(string key, Expression<Func<Measurement, bool>> selector)
		{
			return TryGetMeasurement(key, selector);
		}

		public override async Task<IEnumerable<Measurement>> GetAfterAsync(Sensor sensor, DateTime pit)
		{
			string key;
			IEnumerable<Measurement> measurements;

			key = $"{sensor.InternalId}::after::{pit.ToString(CultureInfo.InvariantCulture)}";
			var data = await _cache.GetAsync(key).AwaitSafely();

			if(data != null)
				return JsonConvert.DeserializeObject<IEnumerable<Measurement>>(data);

			measurements = await base.GetAfterAsync(sensor, pit).AwaitSafely();
			await this.CacheDataAsync(key, measurements, CacheTimeout.TimeoutMedium.ToInt(), false).AwaitSafely();
			return measurements;
		}

		public override IEnumerable<Measurement> GetAfter(Sensor sensor, DateTime pit)
		{
			string key;
			IEnumerable<Measurement> measurements;

			key = $"{sensor.InternalId.ToString()}::after::{pit.ToString()}";
			var data = this._cache.Get(key);

			if(data == null) {
				measurements = base.GetAfter(sensor, pit);
				this.CacheData(key, measurements, CacheTimeout.TimeoutMedium.ToInt(), false);
				return measurements;
			}

			return JsonConvert.DeserializeObject<IEnumerable<Measurement>>(data);

		}

		public override IEnumerable<Measurement> TryGetBetween(Sensor sensor, DateTime start, DateTime end)
		{
			string key, data;
			IEnumerable<Measurement> measurements;

			key = $"{sensor.InternalId.ToString()}::{start.ToString()}::{end.ToString()}";
			data = this._cache.Get(key);

			if(data == null) {
				measurements = base.TryGetBetween(sensor, start, end);
				this.CacheData(key, measurements, CacheTimeout.Timeout.ToInt());
				return measurements;
			}

			return JsonConvert.DeserializeObject<IEnumerable<Measurement>>(data);
		}

		public async override Task<IEnumerable<Measurement>> TryGetBetweenAsync(Sensor sensor, DateTime start, DateTime end)
		{
			string key, data;
			IEnumerable<Measurement> measurements;

			key = $"{sensor.InternalId}::{start.ToString(CultureInfo.InvariantCulture)}::{end.ToString(CultureInfo.InvariantCulture)}";
			data = await _cache.GetAsync(key).AwaitSafely();

			if (data != null)
				return JsonConvert.DeserializeObject<IEnumerable<Measurement>>(data);

			measurements = await base.TryGetBetweenAsync(sensor, start, end).AwaitSafely();
			await CacheDataAsync(key, JsonConvert.SerializeObject(measurements), CacheTimeout.Timeout.ToInt()).AwaitSafely();
			return measurements;
		}

		public override async Task<Measurement> GetMeasurementAsync(string key, Expression<Func<Measurement, bool>> selector)
		{
			return await TryGetMeasurementAsync(key, selector).AwaitSafely();
		}
	}
}
