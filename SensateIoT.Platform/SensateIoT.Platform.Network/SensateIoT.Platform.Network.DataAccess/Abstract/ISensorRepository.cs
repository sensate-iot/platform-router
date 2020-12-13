/*
 * Data repository for sensor information.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface ISensorRepository
	{
		Task<IEnumerable<Sensor>> GetSensorsAsync(CancellationToken ct = default);
		Task<Sensor> GetSensorsByIDAsnc(ObjectId sensorID, CancellationToken ct = default);
	}
}
