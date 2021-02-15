/*
 * Blobs data access repository.
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
	public interface IBlobRepository
	{
		Task<Blob> CreateAsync(Blob blob, CancellationToken ct = default);

		Task<Blob> GetAsync(long blobId, CancellationToken ct = default);
		Task<IEnumerable<Blob>> GetRangeAsync(IList<Sensor> sensors, int skip = -1, int limit = -1, CancellationToken ct = default);
		Task<Blob> GetAsync(string sensorId, string fileName, CancellationToken ct = default);
		Task<IEnumerable<Blob>> GetAsync(string sensorId, int skip = -1, int limit = -1, CancellationToken ct = default);

		Task<Blob> DeleteAsync(string sensorId, string fileName, CancellationToken ct = default);
		Task<bool> DeleteAsync(long id, CancellationToken ct = default);
		Task DeleteAsync(ObjectId sensor, CancellationToken ct = default);
	}
}
