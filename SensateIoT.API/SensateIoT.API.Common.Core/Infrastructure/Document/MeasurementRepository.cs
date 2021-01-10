/*
 * MongoDB measurement repository implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using SensateIoT.API.Common.Core.Exceptions;
using SensateIoT.API.Common.Core.Helpers;
using SensateIoT.API.Common.Core.Infrastructure.Repositories;
using SensateIoT.API.Common.Core.Services.DataProcessing;
using SensateIoT.API.Common.Data.Dto.Generic;
using SensateIoT.API.Common.Data.Enums;
using SensateIoT.API.Common.Data.Models;

namespace SensateIoT.API.Common.Core.Infrastructure.Document
{
	public class MeasurementRepository : AbstractDocumentRepository<MeasurementBucket>, IMeasurementRepository
	{
		private const int MeasurementBucketSize = 500;

		private readonly IGeoQueryService m_geoService;
		protected readonly ILogger<MeasurementRepository> _logger;

		public MeasurementRepository(SensateContext context, IGeoQueryService geo,
									 ILogger<MeasurementRepository> logger) : base(context.Measurements)
		{
			this._logger = logger;
			this.m_geoService = geo;
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

		public virtual async Task<IEnumerable<Measurement>> GetMeasurementsBySensorAsync(
			Sensor sensor, int skip = -1, int limit = -1)
		{
			var fd = Builders<MeasurementBucket>.Filter.Eq(x => x.SensorId, sensor.InternalId);

			try {
				var query = this._collection.Find(fd);

				if(skip >= 0) {
					query = query.Skip(skip);
				}

				if(limit >= 0) {
					query = query.Limit(limit);
				}


				var aslist = await query.ToListAsync().AwaitBackground();

				return this.ConcatMeasurementBuckets(aslist);
			} catch(Exception ex) {
				this._logger.LogWarning(ex.Message);
				return null;
			}
		}

		public virtual async Task DeleteBySensorAsync(Sensor sensor, CancellationToken ct = default)
		{
			try {
				FilterDefinition<MeasurementBucket> fd;

				fd = Builders<MeasurementBucket>.Filter.Eq(x => x.SensorId, sensor.InternalId);
				await this._collection.DeleteManyAsync(fd, ct).AwaitBackground();
			} catch(Exception ex) {
				this._logger.LogWarning(ex.Message);
			}
		}

		public virtual async Task DeleteBetweenAsync(Sensor sensor, DateTime start, DateTime end, CancellationToken ct = default)
		{
			try {
				var builder = Builders<MeasurementBucket>.Filter;
				var filter = builder.ElemMatch(x => x.Measurements,
											   x => x.Timestamp >= start) &
							 builder.ElemMatch(x => x.Measurements,
											   x => x.Timestamp <= end) &
							 builder.Eq(x => x.SensorId, sensor.InternalId);

				await this._collection.DeleteManyAsync(filter, ct).AwaitBackground();
			} catch(Exception ex) {
				this._logger.LogInformation($"Unable to delete measurements: {ex.Message}");
				this._logger.LogDebug(ex.StackTrace);
			}
		}

		public async Task<SingleMeasurement> GetMeasurementAsync(MeasurementIndex index, CancellationToken ct = default)
		{
			var builder = Builders<MeasurementBucket>.Filter;
			var filter = builder.Eq(x => x.InternalId, index.MeasurementBucketId);

			var query = this._collection.Aggregate().Match(filter).Project(projection => new SingleMeasurement {
				Id = this.GenerateId(DateTime.Now),
				SensorId = projection.SensorId,
				Measurement = projection.Measurements.ElementAt(index.Index)
			});

			var rv = await query.FirstOrDefaultAsync(ct).AwaitBackground();
			return rv;
		}

		public virtual async Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsBetweenAsync(
			IEnumerable<Sensor> sensors,
			DateTime start,
			DateTime end,
			int skip = -1,
			int limit = -1,
			OrderDirection order = OrderDirection.None,
			CancellationToken ct = default)
		{
			var ids = new BsonArray();

			foreach(var sensor in sensors) {
				ids.Add(sensor.InternalId);
			}

			var matchTimestamp = new BsonDocument {
				{
					"SensorId", new BsonDocument {
						{"$in", ids}
					}
				}, {
					"First", new BsonDocument {
						{"$lte", end}
					}
				}, {
					"Last", new BsonDocument {
						{"$gte", start}
					}
				}, {
					"Measurements.Timestamp", new BsonDocument {
						{"$gte", start},
						{"$lte", end}
					}
				},
			};

			var projectRewrite = new BsonDocument {
				{"_id", 1},
				{"SensorId", 1},
				{"Timestamp", "$Measurements.Timestamp"},
				{"Location", "$Measurements.Location"},
				{"Data", "$Measurements.Data"},
			};


			var pipeline = new List<BsonDocument> {
				new BsonDocument {{"$match", matchTimestamp}},
				new BsonDocument {{"$unwind", "$Measurements"}},
				new BsonDocument {{"$project", projectRewrite}},
			};

			if(skip > 0) {
				pipeline.Add(new BsonDocument { { "$skip", skip } });
			}

			if(limit > 0) {
				pipeline.Add(new BsonDocument { { "$limit", limit } });
			}

			var query = this._collection.Aggregate<MeasurementsQueryResult>(pipeline, cancellationToken: ct);
			var results = await query.ToListAsync(ct).AwaitBackground();

			return results;
		}

		public virtual async Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsNearAsync(
			IEnumerable<Sensor> sensors,
			DateTime start, DateTime end, GeoJson2DGeographicCoordinates coords,
			int max = 100, int skip = -1, int limit = -1, OrderDirection order = OrderDirection.None, CancellationToken ct = default
		)
		{
			var measurements = await this.GetMeasurementsBetweenAsync(sensors, start, end, ct: ct).AwaitBackground();
			return this.m_geoService.GetMeasurementsNear(measurements.ToList(), coords, max, skip, limit, order, ct);
		}

		public virtual async Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsNearAsync(
			Sensor sensor, DateTime start,
			DateTime end, GeoJson2DGeographicCoordinates coords,
			int max = 100, int skip = -1, int limit = -1,
			OrderDirection order = OrderDirection.None, CancellationToken ct = default)
		{
			var measurements = await this.GetMeasurementsBetweenAsync(sensor, start, end, ct: ct).AwaitBackground();
			return this.m_geoService.GetMeasurementsNear(measurements.ToList(), coords, max, skip, limit, order, ct);
		}

		public async Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsBetweenAsync(
			Sensor sensor, DateTime start, DateTime end,
			int skip = -1, int limit = -1,
			OrderDirection order = OrderDirection.None, CancellationToken ct = default)
		{
			var matchTimestamp = new BsonDocument {
				{
					"SensorId", sensor.InternalId
				}, {
					"First", new BsonDocument {
						{"$lte", end}
					}
				}, {
					"Last", new BsonDocument {
						{"$gte", start}
					}
				}, {
					"Measurements.Timestamp", new BsonDocument {
						{"$gte", start},
						{"$lte", end}
					}
				},
			};

			var projectRewrite = new BsonDocument {
				{"_id", 1},
				{"SensorId", 1},
				{"Timestamp", "$Measurements.Timestamp"},
				{"Location", "$Measurements.Location"},
				{"Data", "$Measurements.Data"},
			};

			var tSort = new BsonDocument {
				{ "Timestamp", order.ToInt() }
			};

			var sort = new BsonDocument {
				{ "$sort", tSort }
			};

			var pipeline = new List<BsonDocument> {
				new BsonDocument {{"$match", matchTimestamp}},
				new BsonDocument {{"$unwind", "$Measurements"}},
				new BsonDocument {{"$project", projectRewrite}},
			};

			if(order != OrderDirection.None) {
				pipeline.Add(sort);
			}

			if(skip > 0) {
				pipeline.Add(new BsonDocument { { "$skip", skip } });
			}

			if(limit > 0) {
				pipeline.Add(new BsonDocument { { "$limit", limit } });
			}

			var query = this._collection.Aggregate<MeasurementsQueryResult>(pipeline, cancellationToken: ct);
			var results = await query.ToListAsync(ct).AwaitBackground();

			return results;
		}

		public virtual async Task<IEnumerable<MeasurementsQueryResult>> GetBetweenAsync(
			Sensor sensor, DateTime start, DateTime end, int skip = -1, int limit = -1, OrderDirection order = OrderDirection.None)
		{
			var data = await this.GetMeasurementsBetweenAsync(sensor, start, end, skip, limit, order).AwaitBackground();
			return data;
		}

		#region Measurement creation

		private static UpdateDefinition<MeasurementBucket> CreateBucketUpdate(
			ObjectId sensor, ICollection<Measurement> measurements)
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

		public async Task StoreAsync(IDictionary<ObjectId, List<Measurement>> measurements, CancellationToken ct)
		{
			var updates = new List<UpdateOneModel<MeasurementBucket>>();
			var total = 0L;

			foreach(var kvpair in measurements) {
				var fbuilder = Builders<MeasurementBucket>.Filter;

				var filter = fbuilder.Eq(x => x.Timestamp, DateTime.Now.ThisHour()) &
							 fbuilder.Eq(x => x.SensorId, kvpair.Key) &
							 fbuilder.Lt(x => x.Count, MeasurementBucketSize);

				for(var idx = 0; idx < kvpair.Value.Count;) {
					var sublist = kvpair.Value.GetRange(idx,
														Math.Min(MeasurementBucketSize, kvpair.Value.Count - idx));
					var update = CreateBucketUpdate(kvpair.Key, sublist);

					var upsert = new UpdateOneModel<MeasurementBucket>(filter, update) {
						IsUpsert = true
					};

					updates.Add(upsert);
					idx += sublist.Count;
					total += idx;
				}
			}

			this._logger.LogDebug("Measurements stored: " + total);

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

		public async Task StoreAsync(Sensor sensor, Measurement measurement,
									 CancellationToken ct = default)
		{
			var dict = new Dictionary<ObjectId, List<Measurement>>();
			var measurements = new List<Measurement> { measurement };

			dict[sensor.InternalId] = measurements;
			await this.StoreAsync(dict, ct).AwaitBackground();
		}

		#endregion
	}
}