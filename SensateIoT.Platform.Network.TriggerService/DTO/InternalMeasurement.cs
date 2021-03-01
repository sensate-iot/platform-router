/*
 * Internal measurement model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using MongoDB.Bson;

namespace SensateIoT.Platform.Network.TriggerService.DTO
{
	public class InternalBulkMeasurements
	{
		public IList<SingleMeasurement> Measurements { get; set; }
		public ObjectId SensorID { get; set; }
	}
}

