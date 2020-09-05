/*
 * Data authorization context repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading.Tasks;

using SensateService.Common.Data.Dto.Authorization;

namespace SensateService.Infrastructure.Repositories
{
	public interface IAuthorizationRepository
	{
		Task<IEnumerable<User>> GetAllUsersAsync();
		Task<IEnumerable<Sensor>> GetAllSensorsAsync();
		Task<IEnumerable<ApiKey>> GetAllSensorKeysAsync();
		Task<ApiKey> GetSensorKeyAsync(string keyValue);
		Task<User> GetUserAsync(string userId);
	}
}
