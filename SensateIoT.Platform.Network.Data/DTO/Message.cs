/*
 * Message data model.
 *
 * @author Michel Megens
 * @email michel@michelmegens.net
 */

using System;

using MongoDB.Bson;
using Newtonsoft.Json;

using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.Converters;
using SensateIoT.Platform.Network.Data.Enums;

namespace SensateIoT.Platform.Network.Data.DTO
{
	public class Message : IPlatformMessage
	{
		[JsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId SensorId { get; set; }
		[JsonRequired]
		public string Data { get; set; }
		public DateTime Timestamp { get; set; }
		public DateTime PlatformTimestamp { get; set; }
		public double Longitude { get; set; }
		public double Latitude { get; set; }

		[JsonIgnore]
		public ObjectId SensorID => this.SensorId;

		[JsonIgnore]
		public MessageType Type => MessageType.Message;
		public MessageEncoding Encoding { get; set; }
	}
}
