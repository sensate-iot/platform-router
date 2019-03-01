/*
 * Sensor statistics implementation.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
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

#region Entry creation

		public async Task IncrementAsync(Sensor sensor)
		{
			SensorStatisticsEntry entry;
			var update = Builders<SensorStatisticsEntry>.Update;
			UpdateDefinition<SensorStatisticsEntry> updateDefinition;

			entry = await this.GetByDateAsync(sensor, DateTime.Now.ThisHour()).AwaitBackground() ??
					await this.CreateForAsync(sensor).AwaitBackground();
			updateDefinition = update.Inc(x => x.Measurements, 1);
			await this._stats.FindOneAndUpdateAsync(x => x.InternalId == entry.InternalId, updateDefinition).AwaitBackground();
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