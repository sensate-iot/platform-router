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

using MongoDB.Driver;

using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Contexts;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class MessageRepository : IMessageRepository
	{
		private readonly IMongoCollection<Message> m_messages;

		public MessageRepository(MongoDBContext ctx)
		{
			this.m_messages = ctx.Messages;
		}

		public Task CreateRangeAsync(IEnumerable<Message> messages, CancellationToken ct = default)
		{
			return this.m_messages.InsertManyAsync(messages, cancellationToken: ct);
		}

		public Task DeleteBySensorAsync(Sensor sensor, CancellationToken ct = default)
		{
			var filter = Builders<Message>.Filter.Eq(x => x.SensorId, sensor.InternalId);
			return this.m_messages.DeleteManyAsync(filter, ct);
		}

		public async Task DeleteBySensorAsync(Sensor sensor, DateTime start, DateTime end, CancellationToken ct = default)
		{
			var builder = Builders<Message>.Filter;
			var filter = builder.Eq(x => x.SensorId, sensor.InternalId) &
						 builder.Gte(x => x.Timestamp, start) &
						 builder.Lte(x => x.Timestamp, end);

			await this.m_messages.DeleteManyAsync(filter, ct).ConfigureAwait(false);
		}
	}
}
