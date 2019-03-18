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
using SensateService.Infrastructure.Repositories;
using SensateService.Helpers;

namespace SensateService.Infrastructure.Document
{
	public class MeasurementRepository : AbstractDocumentRepository<Measurement>, IMeasurementRepository, IBulkWriter<Measurement>
	{
		protected readonly ILogger<MeasurementRepository> _logger;

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

		public virtual async Task DeleteAsync(string id)
		{
			ObjectId objectId;

			objectId = ToInternalId(id);
			await this._collection.DeleteOneAsync(x => x.InternalId == objectId).AwaitBackground();
		}

#region Linq getters

		public virtual async Task<IEnumerable<Measurement>> GetMeasurementsAsync(Expression<Func<Measurement, bool>> expression)
		{
			var result = await this._collection.FindAsync(expression).AwaitBackground();

			if(result == null)
				return null;

			return await result.ToListAsync().AwaitBackground();
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

		public async Task CreateRangeAsync(IEnumerable<Measurement> objs, CancellationToken token)
		{
			var measurements = objs.ToList();
			var concern = WriteConcern.Unacknowledged;
			var db = this._collection.WithWriteConcern(concern);

			var opts = new InsertManyOptions {
				IsOrdered = false,
				BypassDocumentValidation = true
			};

			measurements.ForEach(m => {
				m.InternalId = base.GenerateId(m.CreatedAt);
			});

			await db.InsertManyAsync(measurements, opts, token).AwaitBackground();
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

		public virtual async Task<IEnumerable<Measurement>> GetBetweenAsync(Sensor sensor, DateTime start, DateTime end)
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

		public virtual async Task<Measurement> GetMeasurementAsync(Expression<Func<Measurement, bool>> expression)
		{
			IAsyncCursor<Measurement> result;

			result = await this._collection.FindAsync(expression).AwaitBackground();

			if(result == null)
				return null;

			return await result.FirstOrDefaultAsync().AwaitBackground();
		}
	}
}
