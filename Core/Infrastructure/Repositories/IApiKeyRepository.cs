/*
 * API key data layer.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SensateService.Enums;
using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IApiKeyRepository
	{
		Task CreateAsync(SensateApiKey key, CancellationToken token = default(CancellationToken));
		Task CreateSensorKey(SensateApiKey key, Sensor sensor, CancellationToken token = default);
		Task<SensateApiKey> GetByKeyAsync(string key, CancellationToken token = default(CancellationToken));
		Task<SensateApiKey> GetByIdAsync(string id, CancellationToken token = default(CancellationToken));
		Task MarkRevokedAsync(SensateApiKey key, CancellationToken token = default(CancellationToken));
		Task MarkRevokedRangeAsync(IEnumerable<SensateApiKey> keys, CancellationToken token = default(CancellationToken));
		Task<SensateApiKey> RefreshAsync(SensateApiKey apikey, string key, CancellationToken token = default);
		Task<SensateApiKey> RefreshAsync(SensateApiKey key, CancellationToken token = default(CancellationToken));
		Task<SensateApiKey> RefreshAsync(string id, CancellationToken token = default(CancellationToken));
		string GenerateApiKey();
		Task<IEnumerable<SensateApiKey>> GetByUserAsync(SensateUser user, CancellationToken token = default(CancellationToken));
		Task<IEnumerable<SensateApiKey>> GetByUserAsync(SensateUser user, ApiKeyType type,
			CancellationToken token = default(CancellationToken));
		Task DeleteAsync(SensateUser user, string key, CancellationToken ct = default);
	}
}