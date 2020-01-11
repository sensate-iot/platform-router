/*
 * MongoDB measurement repository implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Linq;

using SensateService.Exceptions;
using SensateService.Models;
using SensateService.Infrastructure.Repositories;
using SensateService.Helpers;

namespace SensateService.Infrastructure.Document
{
	public class MeasurementRepository : AbstractDocumentRepository<MeasurementBucket>, IMeasurementRepository
	{
		private const int MeasurementBucketSize = 500; 
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

		public IEnumerable<Measurement> ConcatMeasurementBuckets(IList<MeasurementBucket> buckets)
		{
			var count = buckets.Aggregate(0, (current, bucket) => current + bucket.Count);
			var data = new List<Measurement>(count);

			foreach(var bucket in buckets) {
				data.AddRange(bucket.Measurements);
			}

			return data.OrderBy(x => x.Timestamp).ToList();
		}

		public virtual async Task<IEnumerable<Measurement>> GetMeasurementsBySensorAsync(Sensor sensor, int skip = -1, int limit = -1)
		{
			var fd = Builders<MeasurementBucket>.Filter.Eq(x => x.SensorId, sensor.InternalId);

			try {
				var query = this._collection.Find(fd);

				if(skip >= 0)
					query = query.Skip(skip);

				if(limit >= 0)
					query = query.Limit(limit);


				var aslist = await query.ToListAsync().AwaitBackground();

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

		public virtual async Task<IEnumerable<Measurement>> GetMeasurementsAsync(Expression<Func<MeasurementBucket, bool>> expression, int skip = -1, int limit = -1)
		{
			var query = this._collection.Find(expression);

			if(skip >= 0)
				query = query.Skip(skip);

			if(limit >= 0)
				query = query.Limit(limit);

			var data = await query.ToListAsync().AwaitBackground();
			return ConcatMeasurementBuckets(data);
		}

		public async Task<long> GetMeasurementCountAsync(Sensor sensor, CancellationToken token = default(CancellationToken))
		{
			var total = this._collection.AsQueryable().Where(x => x.SensorId == sensor.InternalId).SumAsync(x => x.Count, token);
			return await total.AwaitBackground();
		}

		public virtual async Task<IEnumerable<Measurement>> GetMeasurementsAsync(Expression<Func<MeasurementBucket, bool>> expression, Func<Measurement, bool> mexpr, int skip = -1, int limit = -1)
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

#endregion

#region Measurement creation

		private static UpdateDefinition<MeasurementBucket> CreateBucketUpdate(ObjectId sensor, ICollection<Measurement> measurements)
		{
			var first = measurements.ElementAt(0);
			var last = measurements.ElementAt(measurements.Count - 1);

			var ubuilder = Builders<MeasurementBucket>.Update;
			var update = ubuilder.PushEach(x => x.Measurements, measurements)
				.SetOnInsert(x => x.Timestamp, DateTime.Now.ThisHour())
				.SetOnInsert(x => x.SensorId, sensor)
				.SetOnInsert(x => x.First, first.Timestamp)
				.Set(x => x.Last, last.Timestamp)
				.Inc(x => x.Count, measurements.Count);

			return update;
		}

		public async Task StoreAsync(IDictionary<Sensor, List<Measurement>> measurements, CancellationToken ct)
		{
			var updates = new List<UpdateOneModel<MeasurementBucket>>();
			var total = 0L;

			foreach(var kvpair in measurements) {
				var fbuilder = Builders<MeasurementBucket>.Filter;

				var filter = fbuilder.Eq(x => x.Timestamp, DateTime.Now.ThisHour()) &
				             fbuilder.Eq(x => x.SensorId, kvpair.Key.InternalId) &
				             fbuilder.Lt(x => x.Count, MeasurementBucketSize);

				for(var idx = 0; idx < kvpair.Value.Count;) {
					var sublist = kvpair.Value.GetRange(idx,
						Math.Min(MeasurementBucketSize, kvpair.Value.Count - idx));
					var update = CreateBucketUpdate(kvpair.Key.InternalId, sublist);

					var upsert = new UpdateOneModel<MeasurementBucket>(filter, update) {
						IsUpsert = true
					};

					updates.Add(upsert);
					idx += sublist.Count;
					total += idx;
				}
			}

			this._logger.LogDebug("Measurements stored: "+ total);

			var opts = new BulkWriteOptions {
				IsOrdered = false,
				BypassDocumentValidation = true
			};

			try {
				await this._collection.BulkWriteAsync(updates, opts, ct).AwaitBackground();
			} catch(Exception ex) {
				throw new DatabaseException(ex.Message, "Measurements", ex);
			}
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

			if(start.HasValue && end.HasValue) {
				fd &= builder.Gte(x => x.Timestamp, start.Value.ThisHour()) &
				      builder.Lte(x => x.Timestamp, end.Value.ThisHour());
			} else if(!end.HasValue) {
				/* Interpret end == null as infinity and
				   build an 'after' _start_ filter. */

				fd = builder.Eq(x => x.SensorId, sensor.InternalId) &
				     builder.Gte(x => x.Timestamp, start.Value.ThisHour());
			} else {
				fd = builder.Eq(x => x.SensorId, sensor.InternalId) &
				     builder.Lte(x => x.Timestamp, end.Value.AddHours(1D).ThisHour());
			}

			return fd;
		}

		private IEnumerable<Measurement> ConcatMeasurementBuckets(IEnumerable<MeasurementBucket> buckets, int skip,
			int limit)
		{
			List<Measurement> measurements;

			if(skip > 0)
				buckets = buckets.Skip(skip);

			if(limit > 0)
				buckets = buckets.ToList().GetRange(0, limit);

			measurements = new List<Measurement>();

			foreach(var bucket in buckets) {
				measurements.AddRange(bucket.Measurements);
			}

			return measurements;
		}

		private async Task<IEnumerable<Measurement>> LookupAsync(FilterDefinition<MeasurementBucket> fd, DateTime start, DateTime end, int skip, int limit)
		{
			Stopwatch sw = Stopwatch.StartNew();
			IAggregateFluent<MeasurementBucket> query;

			if(start != DateTime.MinValue && end != DateTime.MinValue) {
				query = this._collection.Aggregate().Match(fd)
					.Project(x => new MeasurementBucket {
						Measurements = x.Measurements.Where((measurement) =>
							measurement.Timestamp >= start && measurement.Timestamp <= end),
						SensorId = x.SensorId,
						Timestamp = x.Timestamp,
						Count = x.Count,
						_id = x.InternalId
					});


			} else if(end == DateTime.MinValue) {
				query = this._collection.Aggregate().Match(fd)
					.Project(x => new MeasurementBucket {
						Measurements = x.Measurements.Where((measurement) => measurement.Timestamp >= start),
						SensorId = x.SensorId,
						Timestamp = x.Timestamp,
						Count = x.Count,
						_id = x.InternalId
					});

			} else if(start == DateTime.MinValue) {
				query = this._collection.Aggregate().Match(fd)
					.Project(x => new MeasurementBucket {
						Measurements = x.Measurements.Where(( measurement) => measurement.Timestamp <= end),
						SensorId = x.SensorId,
						Timestamp = x.Timestamp,
						Count = x.Count,
						_id = x.InternalId
					});
			} else {
				query = this._collection.Aggregate().Match(fd)
					.Project(x => new MeasurementBucket {
						Measurements = x.Measurements,
						SensorId = x.SensorId,
						Timestamp = x.Timestamp,
						Count = x.Count,
						_id = x.InternalId
					});
			}

			var buckets = new List<MeasurementBucket>();
			var cursor = await query.ToCursorAsync().AwaitBackground();

			while(await cursor.MoveNextAsync().AwaitBackground()) {
				buckets.AddRange(cursor.Current);
			}

			sw.Stop();
			
			this._logger.LogInformation($"Query took {sw.ElapsedMilliseconds}ms!");
			return this.ConcatMeasurementBuckets(buckets, skip, limit);

		}

		public virtual async Task<IEnumerable<Measurement>> GetBetweenAsync(Sensor sensor, DateTime start, DateTime end, int skip = -1, int limit = -1)
		{
			var data = await this.LookupAsync(BuildQuery(sensor, start, end), start, end, skip, limit).AwaitBackground();
			return data;
		}

		public virtual async Task<IEnumerable<Measurement>> GetBeforeAsync(Sensor sensor, DateTime pit, int skip = -1, int limit = -1)
		{
			var data = await this.LookupAsync(BuildQuery(sensor, null, pit), DateTime.MinValue, pit, skip, limit).AwaitBackground();
			return data;
		}

		public virtual async Task<IEnumerable<Measurement>> GetAfterAsync(Sensor sensor, DateTime pit, int skip = -1, int limit = -1)
		{
			var data = await this.LookupAsync(BuildQuery(sensor, pit, null), pit, DateTime.MinValue, skip, limit).AwaitBackground();
			return data;
		}
#endregion
	}
}
