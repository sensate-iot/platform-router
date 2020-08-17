/*
 * API key data layer.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SensateService.Common.Data.Dto.Json.Out;
using SensateService.Common.Data.Models;
using SensateService.Common.IdentityData.Enums;
using SensateService.Common.IdentityData.Models;

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
		Task<SensateApiKey> RefreshAsync(SensateApiKey key, CancellationToken token = default);
		Task<SensateApiKey> RefreshAsync(string id, CancellationToken token = default);
		string GenerateApiKey();
		Task<IEnumerable<SensateApiKey>> GetByUserAsync(SensateUser user, int skip = 0, int limit = 0, CancellationToken token = default(CancellationToken));
		Task<SensateApiKey> GetAsync(string key, CancellationToken token = default(CancellationToken));
		Task<IEnumerable<SensateApiKey>> GetByUserAsync(SensateUser user, ApiKeyType type,
														int skip = 0, int limit = 0,
														CancellationToken token = default);

		Task<PaginationResult<SensateApiKey>> FilterAsync(SensateUser user, IList<ApiKeyType> types, string query = null, bool revoked = true, int skip = 0, int limit = 0);

		Task IncrementRequestCountAsync(SensateApiKey key, CancellationToken ct = default);
		Task DeleteAsync(SensateUser user, CancellationToken ct = default);
		Task DeleteAsync(string key, CancellationToken ct = default);
	}
}