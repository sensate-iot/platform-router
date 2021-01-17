/*
 * Control message DTO 
 */

using System;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.Converters;

namespace SensateIoT.Platform.Network.Data.DTO
{
	public class ControlMessage : IPlatformMessage
	{
		[JsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId SensorId { get; set; }

		public string Data { get; set; }
		[BsonIgnore]
		public string Secret { get; set; }
		public DateTime Timestamp { get; set; }

		[BsonIgnore, JsonIgnore]
		public ObjectId SensorID => this.SensorId;
		[BsonIgnore, JsonIgnore]
		public MessageType Type => MessageType.ControlMessage;
		[BsonIgnore, JsonIgnore]
		public ControlMessageType Destination { get; set; }
		[BsonIgnore, JsonIgnore]
		public DateTime PlatformTimestamp { get; set; }
	}
}
