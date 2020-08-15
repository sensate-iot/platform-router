/*
 * Standard (non-cached) sensor repository.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;
using MongoDB.Bson;
using SensateService.Enums;
using SensateService.Exceptions;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Document
{
	public class SensorRepository : AbstractDocumentRepository<Sensor>, ISensorRepository
	{
		private readonly IMongoCollection<Sensor> _sensors;
		private readonly ILogger<SensorRepository> _logger;
		private readonly Random _rng;

		private const int SensorSecretLength = 32;

		public SensorRepository(SensateContext context, ILogger<SensorRepository> logger) : base(context.Sensors)
		{
			this._sensors = context.Sensors;
			this._logger = logger;
			this._rng = new Random(DateTime.Now.Millisecond);
		}

		public override async Task CreateAsync(Sensor sensor, CancellationToken ct = default(CancellationToken))
		{
			var now = DateTime.Now;

			sensor.CreatedAt = now;
			sensor.UpdatedAt = now;
			sensor.InternalId = this.GenerateId(now);

			if(string.IsNullOrEmpty(sensor.Secret))
				sensor.Secret = this._rng.NextStringWithSymbols(SensorSecretLength);

			await base.CreateAsync(sensor, ct).AwaitBackground();
		}

		public virtual async Task<IEnumerable<Sensor>> GetAsync(SensateUser user, int skip = 0, int limit = 0)
		{
			FilterDefinition<Sensor> filter;
			var id = user.Id;
			var builder = Builders<Sensor>.Filter;

			filter = builder.Where(s => s.Owner == id);
			var sensors = this._sensors.Aggregate().Match(filter);

			if(sensors == null) {
				return null;
			}

			if(skip > 0) {
				sensors = sensors.Skip(skip);
			}

			if(limit > 0) {
				sensors = sensors.Limit(limit);
			}

			return await sensors.ToListAsync().AwaitBackground();
		}

		public virtual async Task<IEnumerable<Sensor>> GetAsync(IEnumerable<string> ids)
		{
			FilterDefinition<Sensor> filter;
			var builder = Builders<Sensor>.Filter;
			var idlist = new List<ObjectId>();

			foreach(var id in ids) {
				if(!ObjectId.TryParse(id, out var parsedId)) {
					continue;
				}

				idlist.Add(parsedId);
			}

			filter = builder.In(x => x.InternalId, idlist);
			var raw = await this._collection.FindAsync(filter).AwaitBackground();
			return raw.ToList();
		}

		public async Task<IEnumerable<Sensor>> FindByNameAsync(SensateUser user, string name, int skip = 0, int limit = 0)
		{
			FilterDefinition<Sensor> filter;
			var builder = Builders<Sensor>.Filter;

			filter = builder.Where(x => x.Name.Contains(name)) &
					 builder.Eq(x => x.Owner, user.Id);
			var sensors = this._collection.Aggregate().Match(filter);

			if(sensors == null) {
				return null;
			}

			if(skip > 0) {
				sensors = sensors.Skip(skip);
			}

			if(limit > 0) {
				sensors = sensors.Limit(limit);
			}

			return await sensors.ToListAsync().AwaitBackground();
		}

		public virtual async Task<Sensor> GetAsync(string id)
		{
			var oid = new ObjectId(id);
			var filter = Builders<Sensor>.Filter.Where(x => x.InternalId == oid);
			var result = await this._sensors.FindAsync(filter).AwaitBackground();

			if(result == null)
				return null;

			return await result.FirstOrDefaultAsync().AwaitBackground();
		}

		public async Task<long> CountAsync(SensateUser user = null)
		{
			FilterDefinition<Sensor> filter;

			if(user == null) {
				return await this._sensors.CountDocumentsAsync(new BsonDocument()).AwaitBackground();
			}

			var builder = Builders<Sensor>.Filter;
			filter = builder.Where(s => s.Owner == user.Id);
			return await this._sensors.CountDocumentsAsync(filter).AwaitBackground();
		}

		public async Task<long> CountAsync(SensateUser user, string name)
		{
			FilterDefinition<Sensor> filter;
			var builder = Builders<Sensor>.Filter;

			filter = builder.Where(x => x.Owner == user.Id && x.Name.Contains(name));
			return await this._sensors.CountDocumentsAsync(filter).AwaitBackground();
		}

		public async Task DeleteAsync(SensateUser user, CancellationToken ct = default)
		{
			if(user == null) {
				return;
			}

			var builder = Builders<Sensor>.Filter;
			var filter = builder.Eq(x => x.Owner, user.Id);

			await this._sensors.DeleteManyAsync(filter, ct).AwaitBackground();
		}

		public virtual async Task DeleteAsync(Sensor sensor, CancellationToken ct = default)
		{
			if(sensor == null) {
				return;
			}

			await this._sensors.DeleteOneAsync(x => x.InternalId == sensor.InternalId, ct).AwaitBackground();
		}

		public virtual async Task UpdateSecretAsync(Sensor sensor, SensateApiKey key)
		{
			var update = Builders<Sensor>.Update.Set(x => x.UpdatedAt, DateTime.Now)
				.Set(x => x.Secret, key.ApiKey);

			if(key.Revoked || key.Type != ApiKeyType.SensorKey)
				return;

			try {
				await this._sensors.FindOneAndUpdateAsync(x => x.InternalId == sensor.InternalId, update)
					.AwaitBackground();
			} catch(Exception ex) {
				throw new DatabaseException(ex.Message, "Sensors", ex);
			}
		}

		public virtual async Task UpdateAsync(Sensor obj)
		{
			var update = Builders<Sensor>.Update.Set(x => x.UpdatedAt, DateTime.Now);

			if(obj.Name != null)
				update = update.Set(x => x.Name, obj.Name);
			if(obj.Description != null)
				update = update.Set(x => x.Description, obj.Description);

			try {
				await this._sensors.FindOneAndUpdateAsync(x => x.InternalId == obj.InternalId, update).AwaitBackground();
			} catch(Exception ex) {
				this._logger.LogInformation($"Unable to update sensor: {ex.Message}");
			}
		}
	}
}
