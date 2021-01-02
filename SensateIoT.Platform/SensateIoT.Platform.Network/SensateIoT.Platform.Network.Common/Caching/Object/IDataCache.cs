/*
 * Object cache to store routing information related to sensors and messages.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MongoDB.Bson;

using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.Data.Models;

using ApiKey = SensateIoT.Platform.Network.Data.DTO.ApiKey;
using Sensor = SensateIoT.Platform.Network.Data.DTO.Sensor;

namespace SensateIoT.Platform.Network.Common.Caching.Object
{
	public interface IDataCache : IDisposable
	{
		Sensor GetSensor(ObjectId id);
		void Append(IEnumerable<Sensor> sensors);
		void Append(IEnumerable<Account> accounts);
		void Append(IEnumerable<Tuple<string, ApiKey>> keys);
		void Append(Sensor sensor);
		void Append(Account account);
		void Append(string key, ApiKey keyObject);

		void Clear();
		void RemoveSensor(ObjectId sensorID);
		void RemoveAccount(Guid accountID);
		void RemoveApiKey(string key);
		Task ScanCachesAsync();

		void AddLiveDataRoute(LiveDataRoute route);
		void RemoveLiveDataRoute(LiveDataRoute route);
		void SyncLiveData(ICollection<LiveDataRoute> data);
		void SetLiveDataRemotes(IEnumerable<LiveDataHandler> remotes);
		void FlushLiveData();
	}
}
