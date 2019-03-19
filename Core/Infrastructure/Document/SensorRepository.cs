/*
 * Standard (non-cached) sensor repository.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Document
{
	public class SensorRepository : AbstractDocumentRepository<Sensor>, ISensorRepository
	{
		private readonly IMongoCollection<Sensor> _sensors;
		private readonly ILogger<SensorRepository> _logger;
		private readonly IMeasurementRepository _measurements;
		private readonly ISensorStatisticsRepository _stats;

		public SensorRepository(SensateContext context,
								ISensorStatisticsRepository statisticsRepository,
								IMeasurementRepository measurements,
								ILogger<SensorRepository> logger) : base(context.Sensors)
		{
			this._sensors = context.Sensors;
			this._logger = logger;
			this._measurements = measurements;
			this._stats = statisticsRepository;
		}

		public override async Task CreateAsync(Sensor sensor, CancellationToken ct = default(CancellationToken))
		{
			var now = DateTime.Now;

			sensor.CreatedAt = now;
			sensor.UpdatedAt = now;
			sensor.InternalId = base.GenerateId(now);
			await base.CreateAsync(sensor, ct).AwaitBackground();
		}

		public virtual async Task<IEnumerable<Sensor>> GetAsync(SensateUser user)
		{
			FilterDefinition<Sensor> filter;
			var id = user.Id;
			var builder = Builders<Sensor>.Filter;

			filter = builder.Where(s => s.Owner == id);
			var sensors = await this._sensors.FindAsync(filter).AwaitBackground();

			if(sensors == null)
				return null;

			return await sensors.ToListAsync().AwaitBackground();
		}

		public virtual async Task<IEnumerable<Sensor>> GetAsync(IEnumerable<string> ids)
		{
			FilterDefinition<Sensor> filter;
			var builder = Builders<Sensor>.Filter;
			var idlist = ids.Select(ObjectId.Parse);

			filter = builder.In(x => x.InternalId, idlist);
			var raw = await this._collection.FindAsync(filter).AwaitBackground();
			return raw.ToList();
		}

		public async Task<IEnumerable<Sensor>> FindByNameAsync(SensateUser user, string name)
		{
			FilterDefinition<Sensor> filter;
			var builder = Builders<Sensor>.Filter;

			filter = builder.Where(x => x.Name.Contains(name));
			var raw = await this._collection.FindAsync(filter).AwaitBackground();
			return raw.ToList();
		}

		public virtual async Task<Sensor> GetAsync(string id)
		{
			ObjectId oid = new ObjectId(id);
			var filter = Builders<Sensor>.Filter.Where(x => x.InternalId == oid);
			var result = await this._sensors.FindAsync(filter).AwaitBackground();

			if(result == null)
				return null;

			return await result.FirstOrDefaultAsync().AwaitBackground();
		}

		public async Task<long> CountAsync(SensateUser user = null)
		{
			FilterDefinition<Sensor> filter;

			if(user == null)
				return await this._sensors.CountDocumentsAsync(new BsonDocument()).AwaitBackground();

			var builder = Builders<Sensor>.Filter;
			filter = builder.Where(s => s.Owner == user.Id);
			return await this._sensors.CountDocumentsAsync(filter).AwaitBackground();
		}

		public virtual async Task RemoveAsync(string id)
		{
			Sensor sensor;
			ObjectId oid = new ObjectId(id);

			sensor = await this._sensors.FindOneAndDeleteAsync(x => x.InternalId == oid).AwaitBackground();

			if(sensor != null) {
				var tasks = new[] {
                    this._measurements.DeleteBySensorAsync(sensor),
                    this._stats.DeleteBySensorAsync(sensor)
				};

				await Task.WhenAll(tasks).AwaitBackground();
			}
		}

		public virtual async Task UpdateAsync(Sensor obj)
		{
			var update = Builders<Sensor>.Update.Set(x => x.UpdatedAt, DateTime.Now);

			if(obj.Name != null)
				update = update.Set(x => x.Name, obj.Name);
			if(obj.Description != null)
				update = update.Set(x => x.Description, obj.Description);
			if(obj.Secret != null)
				update = update.Set(x => x.Secret, obj.Secret);

			try {
				await this._sensors.FindOneAndUpdateAsync(x => x.InternalId == obj.InternalId, update).AwaitBackground();
			} catch(Exception ex) {
				this._logger.LogInformation($"Unable to update sensor: {ex.Message}");
			}
		}
	}
}
