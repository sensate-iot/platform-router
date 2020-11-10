/*
 * Sensor DTO model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;

using MongoDB.Bson;

namespace SensateIoT.Platform.Network.Data.DTO
{
	public class Sensor
	{
		public ObjectId ID { get; set; }
		public string SensorKey { get; set; }
		public Guid AccountID { get; set; }
		public ICollection<RoutingTarget> RouteInformation { get; set; }
		public SensorTrigger TriggerInformation { get; set; }
	}
}
