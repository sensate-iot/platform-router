/*
 * Data repository for sensor information.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface ISensorRepository
	{
		Task CreateAsync(Sensor sensor, CancellationToken ct = default);

		Task<IEnumerable<Sensor>> GetAsync(Guid userId, int skip = 0, int limit = 0);
		Task<Sensor> GetAsync(string id, CancellationToken ct = default);
		Task<Sensor> GetAsync(ObjectId id, CancellationToken ct = default);
		Task<IEnumerable<Sensor>> GetAsync(IEnumerable<string> ids);
		Task<IEnumerable<Sensor>> FindByNameAsync(Guid userId, string name, int skip = 0, int limit = 0);

		Task<long> CountAsync(User user = null);
		Task<long> CountAsync(User user, string name);

		Task DeleteAsync(Guid userId, CancellationToken ct = default);
		Task DeleteAsync(ObjectId sensorId, CancellationToken ct = default);
		Task UpdateAsync(Sensor sensor);
		Task UpdateSecretAsync(ObjectId sensorId, string key);
	}
}
