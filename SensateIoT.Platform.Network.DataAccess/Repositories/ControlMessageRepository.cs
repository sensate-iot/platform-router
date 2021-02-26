/*
 * Control message repository implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;

using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Contexts;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class ControlMessageRepository : IControlMessageRepository
	{
		private readonly IMongoCollection<ControlMessage> _collection;

		public ControlMessageRepository(MongoDBContext ctx)
		{
			this._collection = ctx.ControlMessages;
		}

		public async Task DeleteBeforeAsync(Sensor sensor, DateTime dt, CancellationToken ct = default)
		{
			var builder = Builders<ControlMessage>.Filter;
			var filters = builder.Eq(x => x.SensorId, sensor.InternalId) &
						  builder.Lte(x => x.Timestamp, dt);

			await this._collection.DeleteManyAsync(filters, ct).ConfigureAwait(false);
		}

		public async Task<IEnumerable<ControlMessage>> GetAsync(Sensor sensor, int skip = -1, int take = -1, CancellationToken ct = default)
		{
			var builder = Builders<ControlMessage>.Filter;
			var filters = builder.Eq(x => x.SensorId, sensor.InternalId);
			var query = this._collection.Find(filters);

			if(skip > 0) {
				query = query.Skip(skip);
			}

			if(take > 0) {
				query = query.Limit(take);
			}

			var result = await query.ToListAsync(ct).ConfigureAwait(false);
			return result;
		}

		public async Task<IEnumerable<ControlMessage>> GetAsync(Sensor sensor, DateTime start, DateTime end, int skip = -1, int take = -1, CancellationToken ct = default)
		{
			var builder = Builders<ControlMessage>.Filter;
			var filters = builder.Eq(x => x.SensorId, sensor.InternalId) &
						  builder.Gte(x => x.Timestamp, start) &
						  builder.Lte(x => x.Timestamp, end);
			var query = this._collection.Find(filters);

			if(skip > 0) {
				query = query.Skip(skip);
			}

			if(take > 0) {
				query = query.Limit(take);
			}

			var result = await query.ToListAsync(ct).ConfigureAwait(false);
			return result;
		}

		public async Task CreateAsync(ControlMessage obj, CancellationToken ct)
		{
			await this._collection.InsertOneAsync(obj, default, ct).ConfigureAwait(false);
		}

		public async Task DeleteBySensorAsync(ObjectId sensor, CancellationToken ct = default)
		{
			var builder = Builders<ControlMessage>.Filter;
			var filter = builder.Eq(x => x.SensorId, sensor);

			await this._collection.DeleteManyAsync(filter, ct).ConfigureAwait(false);
		}

		public async Task DeleteBySensorIds(IEnumerable<ObjectId> sensorIds, CancellationToken ct = default)
		{
			var builder = Builders<ControlMessage>.Filter;
			var filter = builder.In(x => x.SensorId, sensorIds);

			await this._collection.DeleteManyAsync(filter, ct).ConfigureAwait(false);
		}
	}
}
