/*
 * Measurement repository interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.DataAccess.Repositories
{
	public interface IMeasurementRepository
	{
		Task StoreAsync(ObjectId sensorId, Measurement measurement, CancellationToken ct = default);
		Task StoreAsync(IDictionary<ObjectId, List<Measurement>> measurements, CancellationToken ct = default);
	}
}
