/*
 * API data access repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System.Threading.Tasks;

using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface IApiKeyRepository
	{
		Task<ApiKey> GetAsync(string key, CancellationToken ct = default);
		Task DeleteAsync(string key, CancellationToken ct = default);
		Task<ApiKey> CreateSensorKeyAsync(Sensor sensor, CancellationToken ct = default);
		Task UpdateAsync(string old, string @new, CancellationToken ct = default);
	}
}
