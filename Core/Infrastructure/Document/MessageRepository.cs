/*
 * Repository abstraction for the Message model.
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
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Document
{
	public class MessageRepository : AbstractDocumentRepository<Message>, IMessageRepository 
	{
		public MessageRepository(SensateContext ctx) : base(ctx.Messages)
		{
		}

		public Task UpdateAsync(Message msg, string newmsg, CancellationToken ct = default)
		{
			var fb = Builders<Message>.Filter;
			var ub = Builders<Message>.Update;
			var filter = fb.Eq(m => msg.InternalId, msg.InternalId);
			var update = ub.Set(m => m.Data, newmsg);

			return this._collection.UpdateOneAsync(filter, update, null, ct);
		}

		public Task DeleteAsync(Message msg, CancellationToken ct = default)
		{
			var fb = Builders<Message>.Filter;
			var filter = fb.Eq(m => msg.InternalId, msg.InternalId);

			return this._collection.DeleteOneAsync(filter, ct);
		}

		public async Task<IEnumerable<Message>> GetAsync(Sensor sensor, int skip = 0, int take = -1, CancellationToken ct = default)
		{
			var fb = Builders<Message>.Filter;

			var filter = fb.Eq(m => m.SensorId, sensor.InternalId);
			var query = this._collection.Find(filter);

			query = query.Skip(0);

			if(take > 0) {
				query = query.Limit(take);
			}

			var results = await query.ToListAsync(cancellationToken:ct).AwaitBackground();
			return results;
		}

		public async Task<IEnumerable<Message>> GetAsync(Sensor sensor, DateTime start, DateTime end, int skip = 0, int take = -1, CancellationToken ct = default)
		{
			var fb = Builders<Message>.Filter;

			var filter = fb.Eq(m => m.SensorId, sensor.InternalId) &
			             fb.Gte(m => m.CreatedAt, start) &
			             fb.Lte(m => m.CreatedAt, end);

			var query = this._collection.Find(filter);

			query = query.Skip(0);

			if(take > 0) {
				query = query.Limit(take);
			}

			var results = await query.ToListAsync(cancellationToken:ct).AwaitBackground();
			return results;
		}

		public async Task<Message> GetAsync(string messageId, CancellationToken ct = default)
		{
			if(!ObjectId.TryParse(messageId, out var objectId)) {
				return null;
			}

			var filter = Builders<Message>.Filter.Where(x => x.InternalId == objectId);
			var result = await this._collection.FindAsync(filter, cancellationToken:ct).AwaitBackground();

			return await result.FirstOrDefaultAsync(cancellationToken: ct).AwaitBackground();
		}
	}
}
