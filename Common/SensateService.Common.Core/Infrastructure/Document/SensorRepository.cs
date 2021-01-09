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
using MongoDB.Driver;
using MongoDB.Bson;

using SensateService.Common.Data.Models;
using SensateService.Common.IdentityData.Models;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;

namespace SensateService.Infrastructure.Document
{
	public class SensorRepository : AbstractDocumentRepository<Sensor>, ISensorRepository
	{
		private readonly IMongoCollection<Sensor> _sensors;
		private readonly Random _rng;

		private const int SensorSecretLength = 32;

		public SensorRepository(SensateContext context) : base(context.Sensors)
		{
			this._sensors = context.Sensors;
			this._rng = new Random(DateTime.Now.Millisecond);
		}

		public override async Task CreateAsync(Sensor sensor, CancellationToken ct = default)
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
	}
}
