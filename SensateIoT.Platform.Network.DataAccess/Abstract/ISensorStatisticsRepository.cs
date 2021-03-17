/*
 * Measurement/message statistics repository.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Data.Enums;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface ISensorStatisticsRepository
	{
		Task IncrementManyAsync(ObjectId sensorId, StatisticsType method, int num, CancellationToken token = default);
	}
}
