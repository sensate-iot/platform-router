using System;

using MongoDB.Bson;

using Newtonsoft.Json;

using SensateIoT.API.Common.Data.Converters;
using SensateIoT.API.Common.Data.Enums;

namespace SensateIoT.API.Common.Data.Dto.Generic
{
	public class Message
	{
		[JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId InternalId { get; set; }
		public DateTime Timestamp { get; set; }
		public DateTime PlatformTimestamp { get; set; }
		[JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId SensorId { get; set; }
		public GeoJsonPoint Location { get; set; }
		public string Data { get; set; }
		public MessageEncoding Encoding { get; set; }
	}
}