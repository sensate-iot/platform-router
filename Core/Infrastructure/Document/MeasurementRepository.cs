/*
 * MongoDB measurement repository implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
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
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Driver.Linq;

using SensateService.Exceptions;
using SensateService.Models;
using SensateService.Infrastructure.Repositories;
using SensateService.Helpers;
using SensateService.Models.Generic;

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

		public async Task<MeasurementIndex> GetMeasurementIndexAsync(ObjectId sensorId, DateTime timestamp, CancellationToken ct = default)
		{
			if(timestamp.Kind != DateTimeKind.Utc) {
				timestamp = timestamp.ToUniversalTime();
			}

			var builder = Builders<MeasurementBucket>.Filter;
			var filter = builder.Eq(bucket => bucket.SensorId, sensorId) &
						 builder.Eq(bucket => bucket.Timestamp, timestamp.ThisHour()) &
			             builder.Lte(bucket => bucket.First, timestamp) &
			             builder.Gte(bucket => bucket.Last, timestamp);

			var query = this._collection.Aggregate().Match(filter).Project(new BsonDocument {
				{"_id", 1},
				{"Index", new BsonDocument {{"$indexOfArray", new BsonArray {"$Measurements.Timestamp", timestamp}}}}
			});

			var data = await query.FirstOrDefaultAsync(ct).AwaitBackground();

			if(data == null)
				return null;

			var idx = new MeasurementIndex {
				MeasurementBucketId = data.GetValue("_id").AsObjectId,
				Index = data.GetValue("Index").AsInt32
			};

			return idx;
		}

		private const double DiameterOfEarth = 6378137d;

		public async Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsNearAsync(Sensor sensor, DateTime start, DateTime end, GeoJson2DGeographicCoordinates coords,
			int max = 100, int skip = -1, int limit = -1, CancellationToken ct = default)
		{
			var near = new BsonDocument {
				{ "near", new BsonDocument {
					{ "type", "Point" },
					{ "coordinates", new BsonArray { coords.Longitude, coords.Latitude } }
				} },
				{ "spherical", true },
				{ "query", new BsonDocument {
					{ "SensorId", sensor.InternalId },
					{ "First", new BsonDocument { { "$lte", end } } },
					{ "Last", new BsonDocument { { "$gte", start } } }
				} },
				{ "distanceField", "Distance" },
				{ "key", "Measurements.Location" }
			};

			if(max >= 0) {
				near.Add(new BsonElement ("maxDistance", max));
			}

			var matchTimestamp = new BsonDocument {
				{ "Measurements.Timestamp", new BsonDocument {
					{"$gte", start},
					{"$lte", end}
				}}
			};

			var projectRewrite = new BsonDocument {
				{ "_id", 1 },
				{ "Timestamp", "$Measurements.Timestamp" },
				{ "Location", "$Measurements.Location" },
				{ "Data", "$Measurements.Data" },
			};

			double radians = max;
			radians /= DiameterOfEarth;

			var centerSphere = new BsonArray {
				new BsonArray { coords.Longitude, coords.Latitude }, radians
			};

			var match = new BsonDocument {
				{
					"Location", new BsonDocument {
						{
							"$geoWithin", new BsonDocument {
								{ "$centerSphere", centerSphere}
							}
						}
					}
				}
			};

			var sort = new BsonDocument {
				{ "Timestamp", 1 }
			};

			var pipeline = new List<BsonDocument> {
				new BsonDocument {{"$geoNear", near}},
				new BsonDocument {{"$unwind", "$Measurements"}},
				new BsonDocument {{"$match", matchTimestamp}},
				new BsonDocument {{"$project", projectRewrite}},
				new BsonDocument {{"$match", match}},
				new BsonDocument {{"$sort", sort}}
			};

			if(skip > 0) {
				pipeline.Add(new BsonDocument { { "$skip", skip }});
			}

			if(limit > 0) {
				pipeline.Add(new BsonDocument { { "$limit", limit }});
			}

			var query = this._collection.Aggregate<MeasurementsQueryResult>(pipeline, cancellationToken: ct);
			var results = await query.ToListAsync(ct).AwaitBackground();

			return results;
		}

		public async Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsBetweenAsync(Sensor sensor, DateTime start, DateTime end,
			int skip = -1, int limit = -1, CancellationToken ct = default)
		{
			var matchTimestamp = new BsonDocument {
				{
					"SensorId", sensor.InternalId
				},
				{
					"First", new BsonDocument {
						{"$lte", end}
					}
				},
				{
					"Last", new BsonDocument {
						{"$gte", start}
					}
				},
				{
					"Measurements.Timestamp", new BsonDocument {
						{"$gte", start},
						{"$lte", end}
					}
				},
			};

			var projectRewrite = new BsonDocument {
				{ "_id", 1 },
				{ "Timestamp", "$Measurements.Timestamp" },
				{ "Location", "$Measurements.Location" },
				{ "Data", "$Measurements.Data" },
			};


			var pipeline = new List<BsonDocument> {
				new BsonDocument {{"$match", matchTimestamp}},
				new BsonDocument {{"$unwind", "$Measurements"}},
				new BsonDocument {{"$project", projectRewrite}},
			};

			if(skip > 0) {
				pipeline.Add(new BsonDocument { { "$skip", skip }});
			}

			if(limit > 0) {
				pipeline.Add(new BsonDocument { { "$limit", limit }});
			}

			var query = this._collection.Aggregate<MeasurementsQueryResult>(pipeline, cancellationToken: ct);
			var results = await query.ToListAsync(ct).AwaitBackground();

			return results;
		}

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

		public virtual async Task<IEnumerable<MeasurementsQueryResult>> GetBetweenAsync(Sensor sensor, DateTime start, DateTime end, int skip = -1, int limit = -1)
		{
			var data = await this.GetMeasurementsBetweenAsync(sensor, start, end, skip, limit).AwaitBackground();
			return data;
		}

		public virtual async Task<IEnumerable<MeasurementsQueryResult>> GetBeforeAsync(Sensor sensor, DateTime pit, int skip = -1, int limit = -1)
		{
			var start = sensor.CreatedAt;
			return await this.GetBetweenAsync(sensor, start, pit, skip, limit).AwaitBackground();
		}

		public virtual async Task<IEnumerable<MeasurementsQueryResult>> GetAfterAsync(Sensor sensor, DateTime pit, int skip = -1, int limit = -1)
		{
			var end = DateTime.Now;
			return await this.GetBetweenAsync(sensor, pit, end, skip, limit);
		}
#endregion
	}
}
