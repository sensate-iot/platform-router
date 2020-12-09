/*
 * Sensor statistics implementation.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;

using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Contexts;
using SensateIoT.Platform.Network.DataAccess.Extensions;
using SensateIoT.Platform.Network.StorageService.DTO;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class SensorStatisticsRepository : ISensorStatisticsRepository
	{
		private readonly IMongoCollection<SensorStatisticsEntry> _stats;

		public SensorStatisticsRepository(MongoDBContext context)
		{
			this._stats = context.SensorStatistics;
		}

		public async Task DeleteBySensorAsync(Sensor sensor, CancellationToken ct = default)
		{
			var filter = Builders<SensorStatisticsEntry>.Filter
				.Eq(x => x.InternalId, sensor.InternalId);
			await this._stats.DeleteManyAsync(filter, ct).ConfigureAwait(false);
		}

		#region Entry creation

		public async Task IncrementManyAsync(ObjectId sensorId, RequestMethod method, int num, CancellationToken token)
		{
			var update = Builders<SensorStatisticsEntry>.Update;
			UpdateDefinition<SensorStatisticsEntry> updateDefinition;

			updateDefinition = update.Inc(x => x.Measurements, num)
				.SetOnInsert(x => x.Method, method);

			var opts = new UpdateOptions { IsUpsert = true };
			try {
				await this._stats.UpdateOneAsync(x => x.SensorId == sensorId &&
														   x.Date == DateTime.Now.ThisHour() && x.Method == method,
					updateDefinition, opts, token).ConfigureAwait(false);
			} catch(Exception ex) {
				throw new DataException("Unable to update measurement statistics!", ex);
			}
		}

		#endregion
	}
}
