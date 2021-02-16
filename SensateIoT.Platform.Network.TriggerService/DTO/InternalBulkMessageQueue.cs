/*
 * Internal measurement model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using MongoDB.Bson;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.TriggerService.DTO
{
	public class InternalBulkMessageQueue
	{
		public IList<Message> Messages { get; set; }
		public ObjectId SensorID { get; set; }
	}
}