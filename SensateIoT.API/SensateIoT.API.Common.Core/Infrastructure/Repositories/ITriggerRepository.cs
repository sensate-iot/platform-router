/*
 * Trigger data access repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace SensateIoT.API.Common.Core.Infrastructure.Repositories
{
	public interface ITriggerRepository
	{
		Task<long> CountAsync(IEnumerable<ObjectId> sensorIds, CancellationToken ct);
	}
}

