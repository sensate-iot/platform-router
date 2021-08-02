/*
 * Sensor trigger information.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using MongoDB.Bson;

namespace SensateIoT.Platform.Router.Data.DTO
{
	public class TriggerRoutingInfo
	{
		public ObjectId SensorID { get; set; }
		public long ActionCount { get; set; }
		public bool TextTrigger { get; set; }
	}
}
