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

using SensateIoT.Platform.Network.Data.Enums;
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Contexts;
using SensateIoT.Platform.Network.DataAccess.Extensions;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public class SensorStatisticsRepository : ISensorStatisticsRepository
	{
		private readonly IMongoCollection<SensorStatisticsEntry> _stats;

		public SensorStatisticsRepository(MongoDBContext context)
		{
			this._stats = context.SensorStatistics;
		}

		#region Entry creation

		public async Task IncrementManyAsync(ObjectId sensorId, StatisticsType method, int num, CancellationToken token)
		{
			var update = Builders<SensorStatisticsEntry>.Update;
			var opts = new UpdateOptions { IsUpsert = true };
			var updateDefinition = update.Inc(x => x.Count, num)
				.SetOnInsert(x => x.Type, method);

			try {
				await this._stats.UpdateOneAsync(x => x.SensorId == sensorId &&
														   x.Timestamp == DateTime.Now.ThisHour() && x.Type == method,
					updateDefinition, opts, token).ConfigureAwait(false);
			} catch(Exception ex) {
				throw new DataException("Unable to update measurement statistics!", ex);
			}
		}

		#endregion
	}
}
