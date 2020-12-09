/*
 * Message's handler. 
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using MongoDB.Bson;
using Newtonsoft.Json;

using SensateIoT.Platform.Network.Data.Converters;

namespace SensateIoT.Platform.Network.StorageService.Models
{
	public class RawMessage
	{
		[JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId SensorId { get; set; }
		public string Secret { get; set; }
		public string Data { get; set; }
	}
}