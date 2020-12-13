/*
 * Measurement/message statistics repository.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;

using SensateIoT.Platform.Network.StorageService.DTO;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface ISensorStatisticsRepository
	{
		Task IncrementManyAsync(ObjectId sensorId, RequestMethod method, int num, CancellationToken token = default);
	}
}
