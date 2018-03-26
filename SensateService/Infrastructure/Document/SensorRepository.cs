/*
 * Standard (non-cached) sensor repository.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;

using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Document
{
	public class SensorRepository : AbstractDocumentRepository<string, Sensor>, ISensorRepository
	{
		private IMongoCollection<Sensor> _sensors;
		private ILogger<SensorRepository> _logger;

		public SensorRepository(
			SensateContext context,
			ILogger<SensorRepository> logger
		) : base(context)
		{
			this._sensors = context.Sensors;
			this._logger = logger;
		}

		public override void Commit(Sensor obj)
		{
		}

		public override Task CommitAsync(Sensor obj)
		{
			return Task.CompletedTask;
		}

		public override void Create(Sensor obj)
		{
			DateTime now;

			now = DateTime.Now;
			obj.CreatedAt = now;
			obj.UpdatedAt = now;
			obj.InternalId = base.GenerateId(now);
			this._sensors.InsertOne(obj);
			this.Commit(obj);
		}

		public override async Task CreateAsync(Sensor sensor)
		{
			var now = DateTime.Now;

			sensor.CreatedAt = now;
			sensor.UpdatedAt = now;
			sensor.InternalId = base.GenerateId(now);
			await this._sensors.InsertOneAsync(sensor);
			await this.CommitAsync(sensor);
		}

		public virtual void Remove(string secret)
		{
			this.Delete(secret);
		}

		public virtual Sensor Get(string id)
		{
			ObjectId oid = new ObjectId(id);
			return this._sensors.Find(x => x.InternalId == oid).FirstOrDefault();
		}

		public virtual async Task<Sensor> GetAsync(string id)
		{
			ObjectId oid = new ObjectId(id);
			var filter = Builders<Sensor>.Filter.Where(x => x.InternalId == oid);
			var result = await this._sensors.FindAsync(filter);

			if(result == null)
				return null;

			return await result.FirstOrDefaultAsync();
		}

		public async virtual Task RemoveAsync(string id)
		{
			await Task.Run(() => this.Delete(id));
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
			await Task.Run(() => this.Update(sensor));
		}

		public override void Delete(string id)
		{
			ObjectId oid = new ObjectId(id);
			this._sensors.DeleteOne(x => x.InternalId == oid);
		}

		public override async Task DeleteAsync(string id)
		{
			ObjectId oid = new ObjectId(id);
			await this._sensors.DeleteOneAsync(x => x.InternalId == oid);
		}

		public override Sensor GetById(string id)
		{
			ObjectId oid = new ObjectId(id);
			var result = this._sensors.Find(x => x.InternalId == oid);

			if(result == null)
				return null;

			return result.FirstOrDefault();
		}
	}
}
