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
using MongoDB.Driver.Linq;
using SensateService.Models;
using SensateService.Infrastructure.Repositories;
using SensateService.Helpers;

namespace SensateService.Infrastructure.Document
{
	public class MeasurementRepository : AbstractDocumentRepository<MeasurementBucket>, IMeasurementRepository
	{
		private const int MeasurementBucketSize = 500; 
		protected readonly ILogger<MeasurementRepository> _logger;
		private readonly IMongoCollection<Measurement> _measurements;

		public MeasurementRepository(SensateContext context, ILogger<MeasurementRepository> logger) : base(context.Measurements)
		{
			this._logger = logger;
			this._measurements = context.MeasurementData;
		}

		private static ObjectId ToInternalId(string id)
		{
			if(!ObjectId.TryParse(id, out var internalId))
				internalId = ObjectId.Empty;

			return internalId;
		}

		public IEnumerable<Measurement> ConcatMeasurementBuckets(IList<MeasurementBucket> buckets)
		{
			var count = buckets.Aggregate(0, (current, bucket) => current + bucket.Count);
			var data = new List<Measurement>(count);

			foreach(var bucket in buckets) {
				data.AddRange(bucket.Measurements);
			}

			return data.OrderBy(x => x.CreatedAt).ToList();
		}

		public IList<Measurement> ConcatMeasurementBuckets(IList<MeasurementBucket> buckets, Func<Measurement, bool> expr)
		{
			var count = buckets.Aggregate(0, (current, bucket) => current + bucket.Count);
			var data = new List<Measurement>(count);

			foreach(var bucket in buckets) {
				data.AddRange(bucket.Measurements.Where(expr));
			}

			return data.OrderBy(x => x.CreatedAt).ToList();
		}

		public virtual async Task<IEnumerable<Measurement>> GetMeasurementsBySensorAsync(Sensor sensor)
		{
			var query = Builders<MeasurementBucket>.Filter.Eq(x => x.SensorId, sensor.InternalId);

			try {
				var result = await this._collection.FindAsync(query).AwaitBackground();
				var aslist = await result.ToListAsync().AwaitBackground();

				return this.ConcatMeasurementBuckets(aslist);
			} catch (Exception ex) {
				this._logger.LogWarning(ex.Message);
				return null;
			}
		}

		public virtual async Task DeleteBySensorAsync(Sensor sensor)
		{
			try {
				FilterDefinition<MeasurementBucket> fd;

				fd = Builders<MeasurementBucket>.Filter.Eq(x => x.SensorId, sensor.InternalId);
				await this._collection.DeleteManyAsync(fd).AwaitBackground();
			} catch(Exception ex) {
				this._logger.LogWarning(ex.Message);
			}
		}

		public virtual async Task DeleteBetweenAsync(Sensor sensor, DateTime start, DateTime end)
		{
			var builder = Builders<MeasurementBucket>.Filter;

			try {
				FilterDefinition<MeasurementBucket> fd;

				fd = builder.Gte(x => x.Timestamp, start.ThisHour()) &
				     builder.Lte(x => x.Timestamp, end.ThisHour()) &
				     builder.Eq(x => x.SensorId, sensor.InternalId);
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

		public virtual async Task<IEnumerable<Measurement>> GetMeasurementsAsync(Expression<Func<MeasurementBucket, bool>> expression)
		{
			var result = await this._collection.FindAsync(expression).AwaitBackground();

			if(result == null)
				return null;

			var data = await result.ToListAsync().AwaitBackground();
			return ConcatMeasurementBuckets(data);
		}

		public async Task<long> GetMeasurementCountAsync(Sensor sensor, CancellationToken token = default(CancellationToken))
		{
			var total = this._collection.AsQueryable().Where(x => x.SensorId == sensor.InternalId).SumAsync(x => x.Count, token);
			return await total.AwaitBackground();
		}

#endregion

#region Measurement creation

		private static UpdateDefinition<MeasurementBucket> CreateBucketUpdate(ObjectId sensor,
			IList<Measurement> measurements)
		{
				var ubuilder = Builders<MeasurementBucket>.Update;
				var update = ubuilder.PushEach(x => x.Measurements, measurements)
					.SetOnInsert(x => x.Timestamp, DateTime.Now.ThisHour())
					.SetOnInsert(x => x.SensorId, sensor)
					.Inc(x => x.Count, measurements.Count)
					.Min(x => x.First, measurements[0].CreatedAt)
					.Max(x => x.Last, measurements[measurements.Count - 1].CreatedAt);

			return update;
		}

		public async Task StoreAsync(IDictionary<Sensor, List<Measurement>> measurements, CancellationToken ct)
		{
			var concern = new WriteConcern(0, new Optional<TimeSpan?>(), false, false);
			var db = this._measurements.WithWriteConcern(concern);
			var writes = new List<InsertOneModel<Measurement>>();

			/*foreach(var kvpair in measurements) {
				var fbuilder = Builders<MeasurementBucket>.Filter;

				var filter = fbuilder.Eq(x => x.Timestamp, DateTime.Now.ThisHour()) &
				             fbuilder.Eq(x => x.SensorId, kvpair.Key.InternalId) &
				             fbuilder.Lt(x => x.Count, MeasurementBucketSize);

				for(var idx = 0; idx < kvpair.Value.Count; idx += MeasurementBucketSize) {
					var sublist = kvpair.Value.GetRange(idx,
						Math.Min(MeasurementBucketSize, kvpair.Value.Count - idx));
					var update = CreateBucketUpdate(kvpair.Key.InternalId, sublist);

					var upsert = new UpdateOneModel<MeasurementBucket>(filter, update) {
						IsUpsert = true
					};

					data.Add(upsert);
				}
			}

			var opts = new BulkWriteOptions {
				IsOrdered = false
			};*/

			foreach(var kvpair in measurements) {
				foreach(var measurements_data in kvpair.Value) {
					measurements_data.CreatedBy = kvpair.Key.InternalId;
					measurements_data.InternalId = $"{Guid.NewGuid().ToString()}:{DateTime.Now.Ticks}";

					var model = new InsertOneModel<Measurement>(measurements_data);
					writes.Add(model);
				}
			}

			var opts = new BulkWriteOptions {
				IsOrdered = true,
				BypassDocumentValidation = true
			};

			await db.BulkWriteAsync(writes, opts, ct).AwaitBackground();
		}

		public async Task StoreAsync(Sensor sensor, Measurement measurement, CancellationToken ct = default(CancellationToken))
		{
			var dict = new Dictionary<Sensor, List<Measurement>>();
			var measurements = new List<Measurement> {measurement};

			dict[sensor] = measurements;
			await this.StoreAsync(dict, ct).AwaitBackground();
		}

		#endregion

#region Time based getters
		private static FilterDefinition<MeasurementBucket> BuildQuery(Sensor sensor, DateTime? start, DateTime? end)
		{
			FilterDefinition<MeasurementBucket> fd;
			var builder = Builders<MeasurementBucket>.Filter;

			if(start == null && end == null)
				return null;

			fd = builder.Eq(doc => doc.SensorId, sensor.InternalId);

			if(start != null && end != null) {
				fd &= builder.Gte(x => x.First, start) &
					 builder.Lte(x => x.First, end);
			} else if(end == null) {
				/* Interpret end == null as infinity and
				   build an 'after' _start_ filter. */

				fd = builder.Eq(x => x.SensorId, sensor.InternalId) &
					builder.Gte(x => x.First, start);
			} else {
				fd = builder.Eq(x => x.SensorId, sensor.InternalId) &
				     builder.Lte(x => x.First, end);
			}

			return fd;
		}

		private async Task<IList<MeasurementBucket>> LookupAsync(FilterDefinition<MeasurementBucket> fd)
		{
			var result = await this._collection.FindAsync(fd).AwaitBackground();
			return await result.ToListAsync().AwaitBackground();
		}

		public virtual async Task<IEnumerable<Measurement>> GetBetweenAsync(Sensor sensor, DateTime start, DateTime end)
		{
			var data = await this.LookupAsync(BuildQuery(sensor, start, end)).AwaitBackground();
			return this.ConcatMeasurementBuckets(data, x => x.CreatedAt >= start && x.CreatedAt <= end);
		}

		public virtual async Task<IEnumerable<Measurement>> GetBeforeAsync(Sensor sensor, DateTime pit)
		{
			var data = await this.LookupAsync(BuildQuery(sensor, null, pit)).AwaitBackground();
			return this.ConcatMeasurementBuckets(data, x => x.CreatedAt <= pit);
		}

		public virtual async Task<IEnumerable<Measurement>> GetAfterAsync(Sensor sensor, DateTime pit)
		{
			var data = await this.LookupAsync(BuildQuery(sensor, pit, null)).AwaitBackground();
			return this.ConcatMeasurementBuckets(data, x => x.CreatedAt >= pit);
		}
#endregion

		public virtual async Task<IEnumerable<Measurement>> GetMeasurementsAsync(Expression<Func<MeasurementBucket, bool>> expression, Func<Measurement, bool> mexpr)
		{
			IAsyncCursor<MeasurementBucket> result;
			var measurements = new List<Measurement>();

			result = await this._collection.FindAsync(expression).AwaitBackground();
			var list = await result.ToListAsync().AwaitBackground();

			foreach(var bucket in list) {
				measurements.AddRange(bucket.Measurements.Where(mexpr).ToList());
			}

			return measurements;
		}
	}
}
