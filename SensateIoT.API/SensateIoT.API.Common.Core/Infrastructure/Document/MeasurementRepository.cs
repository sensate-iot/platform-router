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

using SensateIoT.API.Common.Core.Helpers;
using SensateIoT.API.Common.Core.Infrastructure.Repositories;
using SensateIoT.API.Common.Core.Services.DataProcessing;
using SensateIoT.API.Common.Data.Dto.Generic;
using SensateIoT.API.Common.Data.Enums;
using SensateIoT.API.Common.Data.Models;

using MeasurementsQueryResult = SensateIoT.API.Common.Data.Models.MeasurementsQueryResult;

namespace SensateIoT.API.Common.Core.Infrastructure.Document
{
	public class MeasurementRepository : AbstractDocumentRepository<MeasurementBucket>, IMeasurementRepository
	{
		private readonly IGeoQueryService m_geoService;
		private readonly ILogger<MeasurementRepository> _logger;

		public MeasurementRepository(SensateContext context, IGeoQueryService geo,
									 ILogger<MeasurementRepository> logger) : base(context.Measurements)
		{
			this._logger = logger;
			this.m_geoService = geo;
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
			DateTime start, DateTime end, GeoJsonPoint coords,
			int max = 100,
			int skip = -1, 
			int limit = -1,
			OrderDirection order = OrderDirection.None,
			CancellationToken ct = default
		)
		{
			var measurements = await this.GetMeasurementsBetweenAsync(sensors, start, end, ct: ct).AwaitBackground();
			return this.m_geoService.GetMeasurementsNear(measurements.ToList(), coords, max, skip, limit, order, ct);
		}

		public virtual async Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsNearAsync(
			Sensor sensor, DateTime start,
			DateTime end, GeoJsonPoint coords,
			int max = 100,
			int skip = -1, 
			int limit = -1,
			OrderDirection order = OrderDirection.None,
			CancellationToken ct = default)
		{
			var measurements = await this.GetMeasurementsBetweenAsync(sensor, start, end, ct: ct).AwaitBackground();
			return this.m_geoService.GetMeasurementsNear(measurements.ToList(), coords, max, skip, limit, order, ct);
		}

		private async Task<IEnumerable<MeasurementsQueryResult>> GetMeasurementsBetweenAsync(
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
	}
}