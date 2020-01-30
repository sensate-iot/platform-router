/*
 * Blob repository interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IBlobRepository
	{
		Task CreateAsync(Blob blob, CancellationToken ct = default);

		Task<Blob> GetAsync(long blobId, CancellationToken ct = default);
		Task<IEnumerable<Blob>> GetAsync(string sensorId, int skip = -1, int limit = -1, CancellationToken ct = default);
		Task<IEnumerable<Blob>> GetLikeAsync(string sensorId, string fileName, int skip = -1, int limit = -1, CancellationToken ct = default);
		Task<Blob> GetAsync(string sensorId, string fileName, CancellationToken ct = default);

		Task<bool> DeleteAsync(string sensorId, string fileName, CancellationToken ct = default);
		Task<bool> DeleteAsync(long id, CancellationToken ct = default);
	}
}
