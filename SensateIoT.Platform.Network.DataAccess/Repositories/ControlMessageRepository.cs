/*
 * Control message repository implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

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
