/*
 * Message authorization model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

using MongoDB.Bson;
using Newtonsoft.Json;

using SensateService.Common.Data.Converters;

namespace SensateService.Common.Data.Dto.Authorization
{
	public class Message
	{
		[JsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId SensorId { get; set; }
		[JsonRequired]
		public string Secret { get; set; }
		[JsonRequired]
		public string Data { get; set; }
		public DateTime Timestamp { get; set; }
		public long Longitude { get; set; }
		public long Latitude { get; set; }
	}
}
