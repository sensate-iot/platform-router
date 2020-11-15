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

namespace SensateIoT.Platform.Network.Data.DTO
{
	public class Message : IPlatformMessage
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

		[JsonIgnore]
		public ObjectId SensorID => this.SensorId;

		[JsonIgnore]
		public MessageType Type => MessageType.Measurement;

		public bool Validate(Sensor sensor)
		{
			return this.Secret == sensor.SensorKey;
		}
	}
}
