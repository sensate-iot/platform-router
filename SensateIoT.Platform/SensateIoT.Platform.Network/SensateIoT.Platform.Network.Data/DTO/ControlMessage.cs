/*
 * Control message DTO 
 */

using System;

using MongoDB.Bson;
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
		public string Secret { get; set; }
		public DateTime Timestamp { get; set; }

		[JsonIgnore]
		public ObjectId SensorID => this.SensorId;
		[JsonIgnore]
		public MessageType Type => MessageType.ControlMessage;
		[JsonIgnore]
		public DateTime PlatformTimestamp { get; set; }
	}
}
