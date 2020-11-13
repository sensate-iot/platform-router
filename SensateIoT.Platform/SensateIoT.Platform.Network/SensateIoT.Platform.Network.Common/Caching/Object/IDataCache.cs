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

namespace SensateIoT.Platform.Network.Common.Caching.Object
{
	public interface IDataCache : IDisposable
	{
		Sensor GetSensor(ObjectId id);
		void Append(IEnumerable<Sensor> sensors);
		void Append(IEnumerable<Account> accounts);
		void Append(IEnumerable<ApiKey> keys);
		void Append(Sensor sensor);
		void Append(Account account);
		void Append(ApiKey key);

		void Clear();
		void RemoveSensor(ObjectId sensorID);
		void RemoveAccount(Guid accountID);
		void RemoveApiKey(string key);
		Task ScanCachesAsync();
	}
}
