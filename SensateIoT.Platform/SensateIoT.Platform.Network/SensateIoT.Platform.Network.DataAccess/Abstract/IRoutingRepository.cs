/*
 * Routing repository interface.
 * 
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Data.DTO;

using ApiKey = SensateIoT.Platform.Network.Data.DTO.ApiKey;
using Sensor = SensateIoT.Platform.Network.Data.DTO.Sensor;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface IRoutingRepository
	{
		Task<IEnumerable<Account>> GetAccountsForRoutingAsync(CancellationToken ct = default);
		Task<Account> GetAccountForRoutingAsync(Guid accountId, CancellationToken ct = default);
		Task<IEnumerable<ApiKey>> GetApiKeysAsync(CancellationToken ct = default);
		Task<ApiKey> GetApiKeyAsync(string key, CancellationToken ct = default);
		Task<IEnumerable<Sensor>> GetSensorsAsync(CancellationToken ct = default);
		Task<Sensor> GetSensorsByIDAsnc(ObjectId sensorID, CancellationToken ct = default);
		Task<TriggerRoutingInfo> GetTriggerInfoAsync(ObjectId sensorID, CancellationToken ct);
		Task<IEnumerable<TriggerRoutingInfo>> GetTriggerInfoAsync(CancellationToken ct);
	}
}
