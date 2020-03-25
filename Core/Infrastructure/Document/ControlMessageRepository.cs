using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Document
{
	public class ControlMessageRepository : AbstractDocumentRepository<ControlMessage>, IControlMessageRepository
	{
		public ControlMessageRepository(SensateContext ctx) : base(ctx.ControlMessages)
		{
		}

		public async Task DeleteBeforeAsync(Sensor sensor, DateTime dt, CancellationToken ct = default)
		{
			var builder = Builders<ControlMessage>.Filter;
			var filters = builder.Eq(x => x.SensorId, sensor.InternalId) &
						  builder.Lte(x => x.Timestamp, dt);

			await this._collection.DeleteManyAsync(filters, ct).AwaitBackground();
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

			var result = await query.ToListAsync(ct).AwaitBackground();
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

			var result = await query.ToListAsync(ct).AwaitBackground();
			return result;
		}

		public async Task<ControlMessage> GetAsync(string messageId, CancellationToken ct = default)
		{
			var builder = Builders<ControlMessage>.Filter;
			var filters = builder.Eq(x => x.SensorId.ToString(), messageId);
			var msg = await this._collection.FindAsync(filters, cancellationToken: ct).AwaitBackground();
			return await msg.FirstOrDefaultAsync(ct).AwaitBackground();
		}

		public async Task DeleteBySensorAsync(Sensor sensor, CancellationToken ct = default)
		{
			var builder = Builders<ControlMessage>.Filter;
			var filter = builder.Eq(x => x.SensorId, sensor.InternalId);

			await this._collection.DeleteManyAsync(filter, ct).AwaitBackground();
		}

		public async Task DeleteBySensorAsync(Sensor sensor, DateTime start, DateTime end, CancellationToken ct = default)
		{
			var builder = Builders<ControlMessage>.Filter;
			var filter = builder.Eq(x => x.SensorId, sensor.InternalId) &
			             builder.Gte(x => x.Timestamp, start) &
			             builder.Lte(x => x.Timestamp, end);

			await this._collection.DeleteManyAsync(filter, ct).AwaitBackground();
		}
	}
}
