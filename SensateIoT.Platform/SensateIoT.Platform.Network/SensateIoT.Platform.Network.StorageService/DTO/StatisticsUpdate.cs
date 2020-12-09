/*
 * Statistics update aggregation model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using MongoDB.Bson;

namespace SensateIoT.Platform.Network.StorageService.DTO
{
	internal class StatisticsUpdate
	{
		public int Count { get; }
		public RequestMethod Method { get; }
		public ObjectId SensorId { get; }

		public StatisticsUpdate(RequestMethod method, int count, ObjectId sensorId)
		{
			this.SensorId = sensorId;
			this.Method = method;
			this.Count = count;
		}
	}
}
