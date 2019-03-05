/*
 * MongoDB measurement repository implementation.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;
using MongoDB.Bson;

using SensateService.Models;
using SensateService.Exceptions;
using SensateService.Infrastructure.Repositories;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Models.Json.In;
using SensateService.Infrastructure.Events;

namespace SensateService.Infrastructure.Document
{
	public class MeasurementRepository : AbstractDocumentRepository<Measurement>, IMeasurementRepository, IBulkWriter<Measurement>
	{
		protected readonly ILogger<MeasurementRepository> _logger;
		private const int MaxDatapointLength = 25;

		public event OnMeasurementReceived MeasurementReceived;

		public MeasurementRepository(SensateContext context, ILogger<MeasurementRepository> logger) : base(context.Measurements)
		{
			this._logger = logger;
		}

		private static ObjectId ToInternalId(string id)
		{
			if(!ObjectId.TryParse(id, out var internalId))
				internalId = ObjectId.Empty;

			return internalId;
		}

		public virtual async Task UpdateAsync(Measurement obj)
		{
			bool updating;
			var update = Builders<Measurement>.Update;
			UpdateDefinition<Measurement> updateDefinition = null;

			updating = false;
			if(Math.Abs(obj.Longitude) > double.Epsilon && Math.Abs(obj.Latitude) > double.Epsilon) {
				updateDefinition = update.Set(x => x.Latitude, obj.Latitude)
				                         .Set(x => x.Longitude, obj.Longitude);
				updating = true;
			}

			if(obj.Data != null)
			{
				updateDefinition = updateDefinition == null ?
					update.Set(x => x.Data, obj.Data) : updateDefinition.Set(x => x.Data, obj.Data);
				updating = true;
			}


			if(!updating)
				return;

			try {
				await this._collection.FindOneAndUpdateAsync( x => x.InternalId == obj.InternalId, updateDefinition );

			} catch(Exception ex) {
				this._logger.LogWarning($"Failed to update measurement: {ex.Message}");
			}
		}

		public virtual async Task<IEnumerable<Measurement>> GetMeasurementsBySensorAsync(Sensor sensor)
		{
			var query = Builders<Measurement>.Filter.Eq("CreatedBy", sensor.InternalId);

			try {
				var result = await this._collection.FindAsync(query).AwaitBackground();
				return await result.ToListAsync().AwaitBackground();
			} catch (Exception ex) {
				this._logger.LogWarning(ex.Message);
				return null;
			}
		}

		public virtual IEnumerable<Measurement> GetMeasurementsBySensor(Sensor sensor)
		{
			var query = Builders<Measurement>.Filter.Eq("CreatedBy", sensor.InternalId);

			try {
				return this._collection.Find(query).ToList();
			} catch (Exception ex) {
				this._logger.LogWarning(ex.Message);
				return null;
			}
		}

		public virtual void DeleteBySensor(Sensor sensor)
		{
			var query = Builders<Measurement>.Filter.Eq("CreatedBy", sensor.InternalId);

			try {
				this._collection.DeleteMany(query);
			} catch(Exception e){
				this._logger.LogWarning(e.Message);
			}
		}

		public virtual async Task DeleteBySensorAsync(Sensor sensor)
		{
			try {
				FilterDefinition<Measurement> fd;

				fd = Builders<Measurement>.Filter.Eq("CreatedBy", sensor.InternalId);
				await this._collection.DeleteManyAsync(fd).AwaitBackground();
			} catch(Exception ex) {
				this._logger.LogWarning(ex.Message);
			}
		}

		public virtual void DeleteBetween(Sensor sensor, DateTime start, DateTime end)
		{
			var builder = Builders<Measurement>.Filter;

			try {
				FilterDefinition<Measurement> fd;

				fd = builder.Gte("CreatedAt", start) & builder.Lte("CreatedAt", end) &
					 builder.Eq("CreatedBy", sensor.InternalId);
				this._collection.DeleteMany(fd);
			} catch(Exception ex) {
				this._logger.LogWarning(ex.Message);
			}
		}

		public virtual async Task DeleteBetweenAsync(Sensor sensor, DateTime start, DateTime end)
		{
			var builder = Builders<Measurement>.Filter;

			try {
				FilterDefinition<Measurement> fd;

				fd = builder.Gte("CreatedAt", start) & builder.Lte("CreatedAt", end) &
				     builder.Eq("CreatedBy", sensor.InternalId);
				await this._collection.DeleteManyAsync(fd).AwaitBackground();
			} catch(Exception ex) {
				this._logger.LogWarning(ex.Message);
			}
		}

		public virtual void Delete(string id)
		{
			ObjectId oid;

			oid = ToInternalId(id);
			this._collection.DeleteOne(x =>
				x.InternalId == oid
			);
		}

		public virtual async Task DeleteAsync(string id)
		{
			ObjectId objectId;

			objectId = ToInternalId(id);
			await this._collection.DeleteOneAsync(x => x.InternalId == objectId).AwaitBackground();
		}

#region Linq getters

		protected virtual Measurement TryGetMeasurement(Expression<Func<Measurement, bool>> expression)
		{
			var result = this._collection.FindSync(expression);
			return result?.FirstOrDefault();
		}

		protected virtual async Task<Measurement> TryGetMeasurementAsync(Expression<Func<Measurement, bool>> expression)
		{
			IAsyncCursor<Measurement> result;

			result = await this._collection.FindAsync(expression).AwaitBackground();

			if(result == null)
				return null;

			return await result.FirstOrDefaultAsync().AwaitBackground();
		}

		public virtual async Task<IEnumerable<Measurement>> TryGetMeasurementsAsync(Expression<Func<Measurement, bool>> expression)
		{
			var result = await this._collection.FindAsync(expression).AwaitBackground();

			if(result == null)
				return null;

			return await result.ToListAsync().AwaitBackground();
		}

		public virtual IEnumerable<Measurement> TryGetMeasurements(Expression<Func<Measurement, bool>> expression)
		{
			var result = this._collection.Find(expression);

			return result?.ToList();
		}

		public async Task<long> GetMeasurementCountAsync(Sensor sensor, CancellationToken token = default(CancellationToken))
		{
			FilterDefinition<Measurement> fd;
			var builder = Builders<Measurement>.Filter;

			fd = builder.Eq("CreatedBy", sensor.InternalId);
			return await this._collection.CountDocumentsAsync(fd, null, token);
		}

#endregion

		public virtual async Task<Measurement> GetByIdAsync(string id)
		{
			ObjectId oid = ToInternalId(id);
			var find = Builders<Measurement>.Filter.Eq("InternalId", oid);
			var result = await this._collection.FindAsync(find).AwaitBackground();

			return result?.FirstOrDefault();
		}

#region Measurement creation

		public override void Create(Measurement m)
		{
			if(m.CreatedBy == ObjectId.Empty)
				return;

			m.CreatedAt = DateTime.Now;
			m.InternalId = this.GenerateId(DateTime.Now);
			base.Create(m);
		}

		public override async Task CreateAsync(Measurement obj, CancellationToken ct = default(CancellationToken))
		{
			if(obj.CreatedBy == ObjectId.Empty)
				return;

			obj.InternalId = this.GenerateId(DateTime.Now);
			await base.CreateAsync(obj, ct).AwaitBackground();
		}

		private async Task<bool> CreateAsync(Sensor sensor, RawMeasurement raw, CancellationToken ct)
		{
			Measurement measurement;
			DateTime timestamp;
			IList<DataPoint> datapoints;
			MeasurementReceivedEventArgs e;

			if(!raw.IsCreatedBy(sensor)) {
				this._logger.LogInformation("Unable to store measurement, invalid secret!");
				return false;
			}

			if(raw.TryParseData(out var data)) {
				datapoints = data as IList<DataPoint>;

				if(datapoints == null)
					throw new InvalidRequestException(ErrorCode.InvalidDataError, "Unable to parse datapoints");

				if(datapoints.Count <= 0 || datapoints.Count > MeasurementRepository.MaxDatapointLength) {
					this._logger.LogInformation($"Invalid number of datapoints in measurement: {datapoints.Count}");
					return false;
				}
			} else {
				throw new InvalidRequestException(ErrorCode.InvalidDataError, "Unable to parse datapoints");
			}

			measurement = new Measurement {
				CreatedBy = sensor.InternalId,
				Data = datapoints,
				Longitude = raw.Longitude,
				Latitude = raw.Latitude
			};

			timestamp = DateTime.Now;
			measurement.CreatedAt = raw.CreatedAt ?? timestamp;
			measurement.InternalId = base.GenerateId(timestamp);

			e = new MeasurementReceivedEventArgs {
				Measurement = measurement,
				CancellationToken = ct
			};

			try {
				var opts = new InsertOneOptions {
					BypassDocumentValidation = true
				};

				var workers = new[] {
					this._collection.InsertOneAsync(measurement, opts, ct),
					this.InvokeReceiveMeasurement(sensor, e)
				};

				await Task.WhenAll(workers).AwaitBackground();
			} catch(Exception ex) {
				this._logger.LogWarning($"Unable to store measurement: {ex.Message}");
				throw new DatabaseException("Unable to store measurement", "Measurements", ex);
			}

			return true;
		}

		public virtual async Task ReceiveMeasurementAsync(Sensor sensor, RawMeasurement measurement)
		{
			CancellationTokenSource src = new CancellationTokenSource();

			try {
				if(Math.Abs(measurement.Latitude) < double.Epsilon &&
				   Math.Abs(measurement.Longitude) < double.Epsilon) {
					throw new InvalidRequestException(
						ErrorCode.InvalidDataError.ToInt(),
						"Invalid measurement location given!"
					);
				}

				var result = await this.CreateAsync(sensor, measurement, src.Token).AwaitBackground();

				if(!result)
					src.Cancel();

			} catch(Exception ex) {
				src.Cancel();
				throw ex;
			} finally {
				src.Dispose();
			}
		}

		public async Task CreateRangeAsync(IEnumerable<Measurement> objs, CancellationToken token)
		{
			var measurements = objs.ToList();
			var opts = new InsertManyOptions {
				IsOrdered = false,
				BypassDocumentValidation = true
			};

			foreach(var measurement in measurements) {
				measurement.InternalId = base.GenerateId(measurement.CreatedAt);
			}

			await this._collection.InsertManyAsync(measurements, opts, token).AwaitBackground();
		}

		private async Task InvokeReceiveMeasurement(Sensor sensor, MeasurementReceivedEventArgs eventargs)
		{
			Delegate[] delegates;

			if(this.MeasurementReceived == null)
				return;

			delegates = this.MeasurementReceived.GetInvocationList();

			if(delegates.Length <= 0)
				return;

			await this.MeasurementReceived.Invoke(sensor, eventargs);
		}

#endregion

#region Time based getters
		private static FilterDefinition<Measurement> BuildFilter(Sensor sensor, DateTime? start, DateTime? end)
		{
			FilterDefinition<Measurement> fd;
			var builder = Builders<Measurement>.Filter;

			if(start == null && end == null)
				return null;

			if(start != null && end != null) {
				fd = builder.Eq("CreatedBy", sensor.InternalId) &
					builder.Gte("CreatedAt", start) & builder.Lte("CreatedAt", end);
			} else if(end == null) {
				/* Interpret end == null as infinity and
				   build an 'after' _start_ filter. */

				fd = builder.Eq("CreatedBy", sensor.InternalId) &
					builder.Gte("CreatedAt", start);
			} else {
				fd = builder.Eq("CreatedBy", sensor.InternalId) &
				     builder.Lte("CreatedAt", end);
			}

			return fd;
		}

		private IEnumerable<Measurement> Lookup(FilterDefinition<Measurement> fd)
		{
			return this._collection.FindSync(fd).ToList();
		}

		private async Task<IEnumerable<Measurement>> LookupAsync(FilterDefinition<Measurement> fd)
		{
			var result = await this._collection.FindAsync(fd).AwaitBackground();
			return await result.ToListAsync().AwaitBackground();
		}

		public virtual IEnumerable<Measurement> TryGetBetween(
			Sensor sensor, DateTime start, DateTime end
		)
		{
			return this.Lookup(BuildFilter(sensor, start, end));
		}

		public virtual async Task<IEnumerable<Measurement>> TryGetBetweenAsync(
			Sensor sensor, DateTime start, DateTime end
		)
		{
			return await this.LookupAsync(BuildFilter(sensor, start, end)).AwaitBackground();
		}

		public virtual IEnumerable<Measurement> GetBefore(Sensor sensor, DateTime pit)
		{
			return this.Lookup(BuildFilter(sensor, null, pit));
		}

		public virtual IEnumerable<Measurement> GetAfter(Sensor sensor, DateTime pit)
		{
			return this.Lookup(BuildFilter(sensor, pit, null));
		}

		public virtual async Task<IEnumerable<Measurement>> GetBeforeAsync(Sensor sensor, DateTime pit)
		{
			return await this.LookupAsync(BuildFilter(sensor, null, pit)).AwaitBackground();
		}

		public virtual async Task<IEnumerable<Measurement>> GetAfterAsync(
			Sensor sensor, DateTime pit
		)
		{
			return await this.LookupAsync(BuildFilter(sensor, pit, null)).AwaitBackground();
		}
#endregion

		public virtual Measurement GetMeasurement(Expression<Func<Measurement, bool>> selector)
		{
			return this.TryGetMeasurement(selector);
		}

		public virtual async Task<Measurement> GetMeasurementAsync(Expression<Func<Measurement, bool>> selector)
		{
			return await this.TryGetMeasurementAsync(selector).AwaitBackground();
		}
	}
}
