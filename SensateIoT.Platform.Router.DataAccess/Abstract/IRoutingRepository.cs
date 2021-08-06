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
using SensateIoT.Platform.Router.Data.DTO;

namespace SensateIoT.Platform.Router.DataAccess.Abstract
{
	public interface IRoutingRepository
	{
		Task<IEnumerable<Account>> GetAccountsForRoutingAsync(CancellationToken ct = default);
		Task<Account> GetAccountForRoutingAsync(Guid accountId, CancellationToken ct = default);
		Task<IEnumerable<Tuple<string, ApiKey>>> GetApiKeysAsync(CancellationToken ct = default);
		Task<ApiKey> GetApiKeyAsync(string key, CancellationToken ct = default);
		Task<IEnumerable<Sensor>> GetSensorsAsync(CancellationToken ct = default);
		Task<Sensor> GetSensorsByIDAsync(ObjectId sensorID, CancellationToken ct = default);
		Task<IEnumerable<TriggerRoutingInfo>> GetTriggerInfoAsync(ObjectId sensorID, CancellationToken ct);
		Task<IEnumerable<TriggerRoutingInfo>> GetTriggerInfoAsync(CancellationToken ct);
	}
}
