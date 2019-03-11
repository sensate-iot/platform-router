/*
 * Sensor statistics implementation.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using MongoDB.Driver;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Document
{
	public class SensorStatisticsRepository : AbstractDocumentRepository<SensorStatisticsEntry>, ISensorStatisticsRepository
	{
		private readonly ILogger<SensorStatisticsRepository> _logger;
		private readonly IMongoCollection<SensorStatisticsEntry> _stats;

		public SensorStatisticsRepository(SensateContext context, ILogger<SensorStatisticsRepository> logger) : base(context.Statistics)
		{
			this._logger = logger;
			this._stats = context.Statistics;
		}

		public async Task<IEnumerable<SensorStatisticsEntry>> GetAsync(Expression<Func<SensorStatisticsEntry, bool>> expr)
		{
			var worker = this._stats.FindAsync(expr);
			var data = await worker.AwaitBackground();
			return data.ToList();
		}

		public void Delete(string id)
		{
			ObjectId objectId;

			objectId = ObjectId.Parse(id);
			this._stats.DeleteOne(x => x.InternalId == objectId);
		}

		public async Task DeleteAsync(string id)
		{
			ObjectId objectId;

			objectId = ObjectId.Parse(id);
			await this._stats.DeleteOneAsync(x => x.InternalId == objectId).AwaitBackground();
		}

		public async Task DeleteBySensorAsync(Sensor sensor)
		{
			var query = Builders<SensorStatisticsEntry>.Filter.Eq("SensorId", sensor.InternalId);

			try {
				await this._stats.DeleteManyAsync(query).AwaitBackground();
			} catch(Exception ex) {
				this._logger.LogWarning(ex.Message);
			}
		}

		public async Task DeleteBySensorAsync(Sensor sensor, DateTime from, DateTime to)
		{
			var f = from.ThisHour();
			var t = to.ThisHour();

			var worker = this._collection.DeleteManyAsync(stat => stat.SensorId == sensor.InternalId &&
			                                                      stat.Date >= f && stat.Date <= t);
			await worker.AwaitBackground();
		}

#region Entry creation

		public Task IncrementAsync(Sensor sensor)
		{
			return this.IncrementManyAsync(sensor, 1, default(CancellationToken));
		}

		public async Task<SensorStatisticsEntry> CreateForAsync(Sensor sensor)
		{
			SensorStatisticsEntry entry;

			entry = new SensorStatisticsEntry {
				InternalId = base.GenerateId(DateTime.Now),
				Date = DateTime.Now.ThisHour(),
				Measurements = 0,
				SensorId = sensor.InternalId
			};

			await this.CreateAsync(entry).AwaitBackground();
			return entry;
		}

		public async Task IncrementManyAsync(Sensor sensor, int num, CancellationToken token)
		{
			SensorStatisticsEntry entry;
			var update = Builders<SensorStatisticsEntry>.Update;
			UpdateDefinition<SensorStatisticsEntry> updateDefinition;

			var entries = await this.GetAfterAsync(sensor, DateTime.Now.ThisHour()).AwaitBackground();

			entry = entries.FirstOrDefault() ??
					await this.CreateForAsync(sensor).AwaitBackground();
			updateDefinition = update.Inc(x => x.Measurements, num);
			await this._stats.FindOneAndUpdateAsync(x => x.InternalId == entry.InternalId, updateDefinition, cancellationToken: token).AwaitBackground();
		}

		#endregion

#region Entry Getters

		public async Task<SensorStatisticsEntry> GetByDateAsync(Sensor sensor, DateTime dt)
		{
			FilterDefinition<SensorStatisticsEntry> filter;
			var filterBuilder = Builders<SensorStatisticsEntry>.Filter;
			var date = dt.ThisHour();

			filter = filterBuilder.Eq("SensorId", sensor.InternalId) & filterBuilder.Eq("Date", date);
			var result = await this._stats.FindAsync(filter).AwaitBackground();

			if(result == null)
				return null;

			return await result.FirstOrDefaultAsync().AwaitBackground();
		}

		public async Task<IEnumerable<SensorStatisticsEntry>> GetBeforeAsync(Sensor sensor, DateTime dt)
		{
			FilterDefinition<SensorStatisticsEntry> filter;
			var filterBuilder = Builders<SensorStatisticsEntry>.Filter;
			var date = dt.ThisHour();

			filter = filterBuilder.Eq("SensorId", sensor.InternalId) & filterBuilder.Lte("Date", date);
			var result = await this._stats.FindAsync(filter).AwaitBackground();

			if(result == null)
				return null;

			return await result.ToListAsync().AwaitBackground();
		}

		public async Task<IEnumerable<SensorStatisticsEntry>> GetAfterAsync(Sensor sensor, DateTime dt)
		{
			FilterDefinition<SensorStatisticsEntry> filter;
			var filterBuilder = Builders<SensorStatisticsEntry>.Filter;
			var date = dt.ThisHour();

			filter = filterBuilder.Eq("SensorId", sensor.InternalId) & filterBuilder.Gte("Date", date);
			var result = await this._stats.FindAsync(filter).AwaitBackground();

			if(result == null)
				return null;

			return await result.ToListAsync().AwaitBackground();
		}

		public async Task<IEnumerable<SensorStatisticsEntry>> GetAfterAsync(DateTime date)
		{
			FilterDefinition<SensorStatisticsEntry> filter;
			var filterBuilder = Builders<SensorStatisticsEntry>.Filter;

			filter = filterBuilder.Gte("Date", date);
			var result = await this._stats.FindAsync(filter).AwaitBackground();

			if(result == null)
				return null;

			return await result.ToListAsync().AwaitBackground();
		}


		public async Task<IEnumerable<SensorStatisticsEntry>> GetBetweenAsync(Sensor sensor, DateTime start, DateTime end)
		{
			FilterDefinition<SensorStatisticsEntry> filter;

			var builder = Builders<SensorStatisticsEntry>.Filter;
			var startDate = start.ThisHour();
			var endDate = end.ThisHour();

			filter = builder.Eq("SensorId", sensor.InternalId) & builder.Gte("Date", startDate) &
			         builder.Lte("Date", endDate);
			var result = await this._stats.FindAsync(filter).AwaitBackground();

			if(result == null)
				return null;

			return await result.ToListAsync().AwaitBackground();
		}

		public SensorStatisticsEntry GetById(string id)
		{
			var fb = Builders<SensorStatisticsEntry>.Filter;

			if(!ObjectId.TryParse(id, out var objId))
				return null;

			var filter = fb.Eq("InternalId", objId);
			var result = this._stats.FindSync(filter);

			return result.FirstOrDefault();
		}

#endregion
	}
}