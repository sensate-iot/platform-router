/*
 * Standard (non-cached) sensor repository.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using SensateService.Models.Repositories;

namespace SensateService.Models.Database
{
	public class StandardSensorRepository : AbstractDocumentRepository<string, Sensor>, ISensorRepository
	{
		private IMongoCollection<Sensor> _sensors;
		private ILogger<StandardSensorRepository> _logger;

		public StandardSensorRepository(
			SensateContext context,
			ILogger<StandardSensorRepository> logger
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

		public override bool Create(Sensor obj)
		{
			this._sensors.InsertOne(obj);
			this.Commit(obj);
			return true;
		}

		public async Task<Boolean> CreateAsync(Sensor sensor)
		{
			await this._sensors.InsertOneAsync(sensor);
			await this.CommitAsync(sensor);
			return true;
		}

		public virtual void Remove(string secret)
		{
			this.Delete(secret);
		}

		public virtual Sensor Get(string id)
		{
			return this._sensors.Find(x => x.Secret == id).FirstOrDefault();
		}

		public virtual async Task<Sensor> GetAsync(string id)
		{
			var result = await this._sensors.FindAsync(x => x.Secret == id);
			return await result.FirstOrDefaultAsync();
		}

		public async virtual Task RemoveAsync(string id)
		{
			await Task.Run(() => this.Delete(id));
		}

		public override bool Replace(Sensor obj1, Sensor obj2)
		{
			obj2.InternalId = obj1.InternalId;
			return this.Update(obj2);
		}

		public override bool Update(Sensor obj)
		{
			var update = Builders<Sensor>.Update
				.Set(x => x.UpdatedAt, DateTime.Now)
				.Set(x => x.Name, obj.Name)
				.Set(x => x.Secret, obj.Secret)
				.Set(x => x.Unit, obj.Unit);

			try {
				this._sensors.FindOneAndUpdate(
					x => x.InternalId == obj.InternalId ||
						x.Secret == obj.Secret,
					update
				);
			} catch(Exception ex) {
				this._logger.LogInformation($"Unable to update sensor: {ex.Message}");
				return false;
			}

			return true;
		}

		public virtual async Task<Boolean> UpdateAsync(Sensor sensor)
		{
			return await Task.Run(() => this.Update(sensor));
		}

		public override bool Delete(string id)
		{
			var result = this._sensors.DeleteOne(x => x.Secret == id);
			return result.DeletedCount == 1;
		}

		public override Sensor GetById(string id)
		{
			return this._sensors.Find(x => x.Secret == id).FirstOrDefault();
		}
	}
}
