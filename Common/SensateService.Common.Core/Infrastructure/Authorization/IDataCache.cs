/*
 * Data cache for sensors, users and API keys.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading.Tasks;

using MongoDB.Bson;

using SensateService.Common.Data.Dto.Authorization;

namespace SensateService.Infrastructure.Authorization
{
	public interface IDataCache
	{
		Sensor GetSensor(ObjectId id);
		void Append(IEnumerable<Sensor> sensors);
		void Append(IEnumerable<User> users);
		void Append(IEnumerable<ApiKey> keys);
		Task Clear();
	}
}