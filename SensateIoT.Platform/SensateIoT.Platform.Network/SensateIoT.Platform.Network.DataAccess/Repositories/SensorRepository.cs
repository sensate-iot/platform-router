/*
 * Data repository for sensor information.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;

using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.DataAccess.Contexts;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class SensorRepository : ISensorRepository
	{
		private readonly IMongoCollection<Data.Models.Sensor> m_sensors;

		public SensorRepository(MongoDBContext ctx)
		{
			this.m_sensors = ctx.Sensors;
		}

		public async Task<IEnumerable<Sensor>> GetSensorsAsync(CancellationToken ct = default)
		{
			var result = new List<Sensor>();

			var query = this.m_sensors.Aggregate()
				.Project(new BsonDocument {
					{"SensorKey", "$Secret"},
					{"AccountID", "$Owner"}
				});
			var cursor = await query.ToCursorAsync(ct).ConfigureAwait(false);

			await cursor.ForEachAsync(document => {
				var sensor = new Sensor {
					AccountID = document["AccountID"].AsGuid,
					ID = document["_id"].AsObjectId,
					SensorKey = document["SensorKey"].AsString
				};

				result.Add(sensor);
			}, ct).ConfigureAwait(false);

			return result;
		}

		public async Task<Sensor> GetSensorsByID(ObjectId sensorID, CancellationToken ct = default)
		{
			var filter = Builders<Data.Models.Sensor>.Filter.Eq(sensor => sensor.InternalId, sensorID);
			var query = this.m_sensors.Aggregate()
				.Match(filter)
				.Project(new BsonDocument {
					{"SensorKey", "$Secret"},
					{"AccountID", "$Owner"}
				});
			var result = await query.FirstOrDefaultAsync(ct).ConfigureAwait(false);

			return new Sensor {
				AccountID = result["AccountID"].AsGuid,
				ID = result["_id"].AsObjectId,
				SensorKey = result["SensorKey"].AsString
			};
		}
	}
}
