/*
 * Message repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface IMessageRepository
	{
		Task CreateRangeAsync(IEnumerable<Message> messages, CancellationToken ct = default);
		Task DeleteBySensorId(ObjectId sensorId, CancellationToken ct = default);
		Task DeleteBySensorId(IEnumerable<ObjectId> sensorIds, CancellationToken ct = default);
	}
}
