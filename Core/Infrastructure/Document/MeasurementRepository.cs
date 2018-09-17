/*
 * MongoDB measurement repository implementation.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;
using MongoDB.Bson;

using SensateService.Infrastructure.Events;
using SensateService.Models;
using SensateService.Exceptions;
using SensateService.Infrastructure.Repositories;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Models.Json.In;

namespace SensateService.Infrastructure.Document
{
	public class MeasurementRepository : AbstractDocumentRepository<string, Measurement>, IMeasurementRepository
	{
		private readonly IMongoCollection<Measurement> _measurements;
		private readonly Random _random;
		protected readonly ILogger<MeasurementRepository> _logger;

		public MeasurementRepository(SensateContext context, ILogger<MeasurementRepository> logger)
			: base(context)
		{
			this._measurements = context.Measurements;
			this._random = new Random();
			this._logger = logger;
		}

		protected ObjectId ToInternalId(string id)
		{
			ObjectId internalId;

			if(!ObjectId.TryParse(id, out internalId))
				internalId = ObjectId.Empty;

			return internalId;
		}

		public override void Update(Measurement obj)
		{
			bool updating;
			var update = Builders<Measurement>.Update;
			UpdateDefinition<Measurement> updateDefinition = null;


			updating = false;
			if(obj.Longitude != 0.0D && obj.Latitude != 0.0D) {
				updateDefinition = update.Set(x => x.Latitude, obj.Latitude)
				                         .Set(x => x.Longitude, obj.Longitude);
				updating = true;
			}

			if(obj.Data != null) {
				if(updateDefinition == null)
					updateDefinition = update.Set(x => x.Data, obj.Data);
				else
					updateDefinition = updateDefinition.Set(x => x.Data, obj.Data);

				updating = true;
			}


			if(!updating)
				return;

			try {
				this._measurements.FindOneAndUpdate(
					x => x.InternalId == obj.InternalId,
					updateDefinition
				);

			} catch(Exception ex) {
				this._logger.LogInformation($"Failed to update measurement: {ex.Message}");
			}
		}

		public virtual async Task<IEnumerable<Measurement>> GetMeasurementsBySensorAsync(Sensor sensor)
		{
			var query = Builders<Measurement>.Filter.Eq("CreatedBy", sensor.InternalId);

			try {
				var result = await this._measurements.FindAsync(query);
				return await result.ToListAsync();
			} catch (Exception ex) {
				this._logger.LogWarning(ex.Message);
				return null;
			}
		}

		public virtual IEnumerable<Measurement> GetMeasurementsBySensor(Sensor sensor)
		{
			var query = Builders<Measurement>.Filter.Eq("CreatedBy", sensor.InternalId);

			try {
				return this._measurements.Find(query).ToList();
			} catch (Exception ex) {
				this._logger.LogWarning(ex.Message);
				return null;
			}
		}

		public virtual void DeleteBySensor(Sensor sensor)
		{
			var query = Builders<Measurement>.Filter.Eq("CreatedBy", sensor.InternalId);

			try {
				this._measurements.DeleteMany(query, default(CancellationToken));
			} catch(Exception e){
				this._logger.LogWarning(e.Message);
			}
		}

		public virtual async Task DeleteBySensorAsync(Sensor sensor)
		{
			FilterDefinition<Measurement> fd;

			try {
				fd = Builders<Measurement>.Filter.Eq("CreatedBy", sensor.InternalId);
				await this._measurements.DeleteManyAsync(fd, default(CancellationToken));
			} catch(Exception ex) {
				this._logger.LogWarning(ex.Message);
			}
		}

		public virtual void DeleteBetween(Sensor sensor, DateTime start, DateTime end)
		{
			var builder = Builders<Measurement>.Filter;
			FilterDefinition<Measurement> fd;

			try {
				fd = builder.Gte("CreatedAt", start) & builder.Lte("CreatedAt", end) & builder.Eq("CreatedBy", sensor.InternalId);
				this._measurements.DeleteMany(fd, default(CancellationToken));
			} catch(Exception ex) {
				this._logger.LogWarning(ex.Message);
			}
		}

		public virtual async Task DeleteBetweenAsync(Sensor sensor, DateTime start, DateTime end)
		{
			var builder = Builders<Measurement>.Filter;
			FilterDefinition<Measurement> fd;

			try {
				fd = builder.Gte("CreatedAt", start) & builder.Lte("CreatedAt", end) & builder.Eq("CreatedBy", sensor.InternalId);
				await this._measurements.DeleteManyAsync(fd, default(CancellationToken));
			} catch(Exception ex) {
				this._logger.LogWarning(ex.Message);
			}
		}

		public override void Delete(string id)
		{
			ObjectId oid;

			oid = this.ToInternalId(id);
			if(oid == null)
				return;

			this._measurements.DeleteOne(x =>
				x.InternalId == oid
			);
		}

		public override async Task DeleteAsync(string id)
		{
			ObjectId objectId;

			objectId = this.ToInternalId(id);
			if(objectId == null)
				return;

			await this._measurements.DeleteOneAsync(x => x.InternalId == objectId);
		}

#region Linq getters

		public virtual Measurement TryGetMeasurement(
			string key,
			Expression<Func<Measurement, bool>> expression
		)
		{
			var result = this._measurements.FindSync(expression);

			if(result == null)
				return null;

			return result.FirstOrDefault();
		}

		public async virtual Task<Measurement> TryGetMeasurementAsync(
			string key, Expression<Func<Measurement, bool>> expression)
		{
			IAsyncCursor<Measurement> result;

			result = await this._measurements.FindAsync(expression);

			if(result == null)
				return null;

			return await result.FirstOrDefaultAsync();
		}

		public async virtual Task<IEnumerable<Measurement>> TryGetMeasurementsAsync(
			string key, Expression<Func<Measurement, bool>> expression)
		{
			var result = await this._measurements.FindAsync(expression);

			if(result == null)
				return null;

			return await result.ToListAsync();
		}

		public virtual IEnumerable<Measurement> TryGetMeasurements(
			string key, Expression<Func<Measurement, bool>> expression)
		{
			var result = this._measurements.Find(expression);

			if(result == null)
				return null;

			return result.ToList();
		}

#endregion

		public override Measurement GetById(string id)
		{
			ObjectId oid = this.ToInternalId(id);
			var find = Builders<Measurement>.Filter.Eq("InternalId", oid);
			var result = this._measurements.Find(find);

			if(result != null)
				return result.FirstOrDefault();

			return null;
		}

#region Measurement creation

		public async Task ReceiveMeasurement(Sensor sender, RawMeasurement measurement)
		{
			MeasurementReceivedEventArgs args;
			Measurement m;

			m = await this.StoreMeasurement(sender, measurement);
			if(m != null) {
				args = new MeasurementReceivedEventArgs() {
					Measurement = m
				};

				await MeasurementEvents.OnMeasurementReceived(sender, args);
			}
		}

		public override void Create(Measurement m)
		{
			if(m.CreatedBy == null || m.CreatedBy == ObjectId.Empty)
				return;

			m.CreatedAt = DateTime.Now;
			m.InternalId = this.GenerateId(DateTime.Now);
			this._measurements.InsertOne(m);
			this.Create(m);
		}

		public async override Task CreateAsync(Measurement obj)
		{
			if(obj.CreatedBy == null || obj.CreatedBy == ObjectId.Empty)
				return;

			obj.CreatedAt = DateTime.Now;
			obj.InternalId = this.GenerateId(DateTime.Now);
			await this._measurements.InsertOneAsync(obj);
			await this.CreateAsync(obj);
		}

		private async Task<Measurement> StoreMeasurement(Sensor sensor, RawMeasurement raw)
		{
			IEnumerable<DataPoint> data;
			Measurement m;
			DateTime now;

			if(sensor == null || raw == null)
				return null;

			now = DateTime.Now;
			m = new Measurement();

			if(raw.CreatedAt == null || raw.CreatedAt.IsNever())
				raw.CreatedAt = now;

			try {
				m.CreatedAt = raw.CreatedAt.Value;
				m.CreatedBy = sensor.InternalId;
				m.Latitude = raw.Latitude;
				m.Longitude = raw.Longitude;
				m.InternalId = base.GenerateId(now);
				data = new List<DataPoint>();

				if(raw.TryParseData(out data)) {
					m.Data = data;
				} else {
					throw new InvalidRequestException(
						ErrorCode.InvalidDataError.ToInt(),
						"Unable to parse datapoints!"
					);
				}

				var options = new InsertOneOptions {
					BypassDocumentValidation = true,
				};

				await this._measurements.InsertOneAsync(m, options, CancellationToken.None);
				await this.CreateAsync(m);
			} catch(InvalidRequestException ex) {
				this._logger.LogWarning(ex.Message);
				return null;
			} catch(Exception ex) {
				this._logger.LogWarning($"Unable to create measurement: ${ex.Message}");
				if(ex is CachingException)
					throw ex;

				throw new DatabaseException(
					$"Failed to store measurement ${ex.Message}",
					"Measurements", ex
				);
			}

			return m;
		}

#endregion
#region Time based getters

		public virtual IEnumerable<Measurement> TryGetBetween(Sensor sensor, DateTime start, DateTime end)
		{
			return this.TryGetMeasurements(null, x =>
				x.CreatedBy == sensor.InternalId &&
				x.CreatedAt.CompareTo(start) >= 0 && x.CreatedAt.CompareTo(end) <= 0
			);
		}

		public virtual async Task<IEnumerable<Measurement>> TryGetBetweenAsync(
			Sensor sensor, DateTime start, DateTime end
		)
		{
			return await this.TryGetMeasurementsAsync(null, x =>
				x.CreatedBy == sensor.InternalId &&
				x.CreatedAt.CompareTo(start) >= 0 && x.CreatedAt.CompareTo(end) <= 0
			);
		}

		public virtual IEnumerable<Measurement> GetBefore(Sensor sensor, DateTime pit)
		{
			string key;

			key = $"{sensor.Secret}::before::{pit.ToString()}";
			return this.TryGetMeasurements(key, x =>
				x.CreatedBy == sensor.InternalId && x.CreatedAt.CompareTo(pit) <= 0
			);
		}

		public virtual IEnumerable<Measurement> GetAfter(Sensor sensor, DateTime pit)
		{
			var result = this._measurements.Find(x =>
				x.CreatedBy == sensor.InternalId && x.CreatedAt.CompareTo(pit) >= 0
			);

			if(result == null)
				return null;

			return result.ToList();
		}

		public virtual async Task<IEnumerable<Measurement>> GetBeforeAsync(Sensor sensor, DateTime pit)
		{
			string key;

			key = $"{sensor.Secret}::before::{pit.ToString()}";
			return await this.TryGetMeasurementsAsync(key, x =>
				x.CreatedBy == sensor.InternalId && x.CreatedAt.CompareTo(pit) <= 0
			);
		}

		public virtual async Task<IEnumerable<Measurement>> GetAfterAsync(Sensor sensor, DateTime pit)
		{
			var result = await this._measurements.FindAsync(x =>
				x.CreatedBy == sensor.InternalId && x.CreatedAt.CompareTo(pit) >= 0
			);

			if(result == null)
				return null;

			return await result.ToListAsync();
		}
#endregion

		public virtual Measurement GetMeasurement(string key, Expression<Func<Measurement, bool>> selector)
		{
			return this.TryGetMeasurement(key, selector);
		}

		public virtual async Task<Measurement> GetMeasurementAsync(string key, Expression<Func<Measurement, bool>> selector)
		{
			return await this.TryGetMeasurementAsync(key, selector);
		}

		public virtual void Commit(Measurement obj)
		{
		}

		public virtual async Task CommitAsync(Measurement obj)
		{
			await Task.CompletedTask;
		}
	}
}
