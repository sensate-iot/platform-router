/*
 * Standard (non-cached) sensor repository.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Document
{
	public class SensorRepository : AbstractDocumentRepository<string, Sensor>, ISensorRepository
	{
		private readonly IMongoCollection<Sensor> _sensors;
		private readonly ILogger<SensorRepository> _logger;
		private readonly IMeasurementRepository _measurements;
		private readonly ISensorStatisticsRepository _stats;

		public SensorRepository(SensateContext context,
								ISensorStatisticsRepository statisticsRepository,
								IMeasurementRepository measurements,
								ILogger<SensorRepository> logger) : base(context)
		{
			this._sensors = context.Sensors;
			this._logger = logger;
			this._measurements = measurements;
			this._stats = statisticsRepository;
		}

		public override void Create(Sensor obj)
		{
			DateTime now;

			now = DateTime.Now;
			obj.CreatedAt = now;
			obj.UpdatedAt = now;
			obj.InternalId = base.GenerateId(now);
			this._sensors.InsertOne(obj);
		}

		public override async Task CreateAsync(Sensor sensor)
		{
			var now = DateTime.Now;

			sensor.CreatedAt = now;
			sensor.UpdatedAt = now;
			sensor.InternalId = base.GenerateId(now);
			await this._sensors.InsertOneAsync(sensor).AwaitSafely();
		}

		public virtual void Remove(string id)
		{
			this.Delete(id);
		}

		public virtual Sensor Get(string id)
		{
			ObjectId oid = new ObjectId(id);
			return this._sensors.Find(x => x.InternalId == oid).FirstOrDefault();
		}

		public virtual async Task<IEnumerable<Sensor>> GetAsync(SensateUser user)
		{
			FilterDefinition<Sensor> filter;
			var id = user.Id;
			var builder = Builders<Sensor>.Filter;

			filter = builder.Where(s => s.Owner == id);
			var sensors = await this._sensors.FindAsync(filter).AwaitSafely();

			if(sensors == null)
				return null;

			return await sensors.ToListAsync().AwaitSafely();
		}

		public virtual async Task<Sensor> GetAsync(string id)
		{
			ObjectId oid = new ObjectId(id);
			var filter = Builders<Sensor>.Filter.Where(x => x.InternalId == oid);
			var result = await this._sensors.FindAsync(filter).AwaitSafely();

			if(result == null)
				return null;

			return await result.FirstOrDefaultAsync().AwaitSafely();
		}

		public async Task<long> CountAsync(SensateUser user = null)
		{
			FilterDefinition<Sensor> filter;

			if(user == null)
				return await this._sensors.CountDocumentsAsync(new BsonDocument()).AwaitSafely();

			var builder = Builders<Sensor>.Filter;
			filter = builder.Where(s => s.Owner == user.Id);
			return await this._sensors.CountDocumentsAsync(filter).AwaitSafely();
		}

		public virtual async Task RemoveAsync(string id)
		{
			await this.DeleteAsync(id).AwaitSafely();
		}

		public override void Update(Sensor obj)
		{
			var update = Builders<Sensor>.Update
				.Set(x => x.UpdatedAt, DateTime.Now);

			if(obj.Name != null)
				update.Set(x => x.Name, obj.Name);
			if(obj.Description != null)
				update.Set(x => x.Description, obj.Description);
			if(obj.Secret != null)
				update.Set(x => x.Secret, obj.Secret);

			try {
				this._sensors.FindOneAndUpdate(
					x => x.InternalId == obj.InternalId ||
						x.Secret == obj.Secret,
					update
				);
			} catch(Exception ex) {
				this._logger.LogInformation($"Unable to update sensor: {ex.Message}");
			}
		}

		public virtual async Task UpdateAsync(Sensor sensor)
		{
			await Task.Run(() => this.Update(sensor)).AwaitSafely();
		}

		public override void Delete(string id)
		{
			Sensor sensor;
			ObjectId oid = new ObjectId(id);

			sensor = this._sensors.FindOneAndDelete(x => x.InternalId == oid);

			if(sensor != null)
				this._measurements.DeleteBySensor(sensor);
		}

		public override async Task DeleteAsync(string id)
		{
			Sensor sensor;
			ObjectId oid = new ObjectId(id);

			sensor = await this._sensors.FindOneAndDeleteAsync(x => x.InternalId == oid).AwaitSafely();

			if(sensor != null) {
				var tasks = new[] {
                    this._measurements.DeleteBySensorAsync(sensor),
                    this._stats.DeleteBySensorAsync(sensor)
				};

				await Task.WhenAll(tasks).AwaitSafely();
			}
		}

		public override Sensor GetById(string id)
		{
			ObjectId oid = new ObjectId(id);
			var result = this._sensors.Find(x => x.InternalId == oid);

			return result?.FirstOrDefault();
		}
	}
}
