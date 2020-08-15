/*
 * Sensor statistics implementation.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;

using SensateService.Enums;
using SensateService.Exceptions;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Document
{
	public class SensorStatisticsRepository : AbstractDocumentRepository<SensorStatisticsEntry>, ISensorStatisticsRepository
	{
		private readonly IMongoCollection<SensorStatisticsEntry> _stats;

		public SensorStatisticsRepository(SensateContext context) : base(context.Statistics)
		{
			this._stats = context.Statistics;
		}

		public async Task<IEnumerable<SensorStatisticsEntry>> GetBetweenAsync(IList<Sensor> sensors, DateTime start, DateTime end)
		{
			var builder = Builders<SensorStatisticsEntry>.Filter;
			var ids = sensors.Select(x => x.InternalId);
			var startHour = start.ThisHour();
			var endHour = end.ThisHour();
			var filter = builder.In(x => x.SensorId, ids) &
						 builder.Gte(x => x.Date, startHour) &
						 builder.Lte(x => x.Date, endHour);

			var result = await this._stats.FindAsync(filter).AwaitBackground();
			return await result.ToListAsync().AwaitBackground();
		}

		public async Task<IEnumerable<SensorStatisticsEntry>> GetAsync(Expression<Func<SensorStatisticsEntry, bool>> expr)
		{
			var worker = this._stats.FindAsync(expr);
			var data = await worker.AwaitBackground();
			return data.ToList();
		}

		public async Task DeleteBySensorAsync(Sensor sensor, CancellationToken ct = default)
		{
			var filter = Builders<SensorStatisticsEntry>.Filter
				.Eq(x => x.InternalId, sensor.InternalId);
			await this._collection.DeleteManyAsync(filter, ct).AwaitBackground();
		}

		public async Task DeleteBySensorAsync(Sensor sensor, DateTime @from, DateTime to, CancellationToken ct = default)
		{
			var startHour = @from.ThisHour();
			var endHour = to.ThisHour();
			var builder = Builders<SensorStatisticsEntry>.Filter;
			var filter = builder.Eq(x => x.InternalId, sensor.InternalId) &
						 builder.Gte(x => x.Date, startHour) &
						 builder.Lte(x => x.Date, endHour);
			await this._collection.DeleteManyAsync(filter, ct).AwaitBackground();
		}

		#region Entry creation

		public Task IncrementAsync(ObjectId sensorId, RequestMethod method)
		{
			return this.IncrementManyAsync(sensorId, method, 1, default);
		}

		public async Task<SensorStatisticsEntry> CreateForAsync(Sensor sensor)
		{
			SensorStatisticsEntry entry;

			entry = new SensorStatisticsEntry {
				InternalId = this.GenerateId(DateTime.Now),
				Date = DateTime.Now.ThisHour(),
				Measurements = 0,
				SensorId = sensor.InternalId
			};

			await this.CreateAsync(entry).AwaitBackground();
			return entry;
		}

		public async Task IncrementManyAsync(ObjectId sensorId, RequestMethod method, int num, CancellationToken token)
		{
			var update = Builders<SensorStatisticsEntry>.Update;
			UpdateDefinition<SensorStatisticsEntry> updateDefinition;

			updateDefinition = update.Inc(x => x.Measurements, num)
				.SetOnInsert(x => x.Method, method);

			var opts = new UpdateOptions { IsUpsert = true };
			try {
				await this._collection.UpdateOneAsync(x => x.SensorId == sensorId &&
														   x.Date == DateTime.Now.ThisHour() && x.Method == method,
					updateDefinition, opts, token).AwaitBackground();
			} catch(Exception ex) {
				throw new DatabaseException("Unable to update measurement statistics!", "Statistics", ex);
			}
		}

		#endregion

		#region Entry Getters

		public async Task<SensorStatisticsEntry> GetByDateAsync(Sensor sensor, DateTime dt)
		{
			FilterDefinition<SensorStatisticsEntry> filter;
			var filterBuilder = Builders<SensorStatisticsEntry>.Filter;
			var date = dt.ThisHour();

			filter = filterBuilder.Eq(x => x.SensorId, sensor.InternalId) & filterBuilder.Eq(x => x.Date, date);
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

			filter = filterBuilder.Eq(x => x.SensorId, sensor.InternalId) & filterBuilder.Lte(x => x.Date, date);
			var result = await this._stats.FindAsync(filter).AwaitBackground();

			if(result == null)
				return null;

			return await result.ToListAsync().AwaitBackground();
		}

		public async Task<IEnumerable<SensorStatisticsEntry>> GetAfterAsync(IEnumerable<Sensor> sensors, DateTime dt)
		{
			FilterDefinition<SensorStatisticsEntry> filter;
			var filterBuilder = Builders<SensorStatisticsEntry>.Filter;
			var date = dt.ThisHour();
			var ids = sensors.Select(x => x.InternalId);

			filter = filterBuilder.In(x => x.SensorId, ids) & filterBuilder.Gte(x => x.Date, date);
			var result = await this._stats.FindAsync(filter).AwaitBackground();

			if(result == null) {
				return null;
			}

			return await result.ToListAsync().AwaitBackground();
		}

		public async Task<IEnumerable<SensorStatisticsEntry>> GetAfterAsync(Sensor sensor, DateTime dt)
		{
			FilterDefinition<SensorStatisticsEntry> filter;
			var filterBuilder = Builders<SensorStatisticsEntry>.Filter;
			var date = dt.ThisHour();

			filter = filterBuilder.Eq(x => x.SensorId, sensor.InternalId) & filterBuilder.Gte(x => x.Date, date);
			var result = await this._stats.FindAsync(filter).AwaitBackground();

			if(result == null)
				return null;

			return await result.ToListAsync().AwaitBackground();
		}

		public async Task<IEnumerable<SensorStatisticsEntry>> GetAfterAsync(DateTime date)
		{
			FilterDefinition<SensorStatisticsEntry> filter;
			var filterBuilder = Builders<SensorStatisticsEntry>.Filter;

			filter = filterBuilder.Gte(x => x.Date, date);
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

			filter = builder.Eq(x => x.SensorId, sensor.InternalId) & builder.Gte(x => x.Date, startDate) &
					 builder.Lte(x => x.Date, endDate);
			var result = await this._stats.FindAsync(filter).AwaitBackground();

			if(result == null)
				return null;

			return await result.ToListAsync().AwaitBackground();
		}
		#endregion
	}
}