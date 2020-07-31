/*
 * Message's handler. 
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using MongoDB.Bson;
using Newtonsoft.Json;

using SensateService.Converters;

namespace SensateService.MqttHandler.Models
{
	public class RawMessage
	{
		[JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId SensorId { get; set; }
		public string Secret { get; set; }
		public string Data { get; set; }
	}
}