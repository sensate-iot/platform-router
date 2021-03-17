/*
 * Statistics update aggregation model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using MongoDB.Bson;
using SensateIoT.Platform.Network.Data.Enums;

namespace SensateIoT.Platform.Network.StorageService.DTO
{
	internal class StatisticsUpdate
	{
		public int Count { get; }
		public StatisticsType Type { get; }
		public ObjectId SensorId { get; }

		public StatisticsUpdate(StatisticsType type, int count, ObjectId sensorId)
		{
			this.SensorId = sensorId;
			this.Type = type;
			this.Count = count;
		}
	}
}
