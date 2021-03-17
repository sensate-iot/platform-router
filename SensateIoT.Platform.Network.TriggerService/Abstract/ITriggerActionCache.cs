using System.Collections.Generic;
using MongoDB.Bson;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.TriggerService.Abstract
{
	public interface ITriggerActionCache
	{
		List<TriggerAction> Lookup(ObjectId sensorId);
		void Load(IEnumerable<TriggerAction> actions);
		void FlushSensor(ObjectId sensorId);
	}
}
