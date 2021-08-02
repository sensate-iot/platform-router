/*
 * Measurement class.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

using MongoDB.Bson;
using Newtonsoft.Json;

using SensateIoT.Platform.Router.Data.Abstract;
using SensateIoT.Platform.Router.Data.Converters;

using DataPointMap = System.Collections.Generic.IDictionary<string, SensateIoT.Platform.Router.Data.DTO.DataPoint>;

namespace SensateIoT.Platform.Router.Data.DTO
{
	public class Measurement : IPlatformMessage
	{
		[JsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId SensorId { get; set; }
		public decimal Longitude { get; set; }
		public decimal Latitude { get; set; }
		public DateTime Timestamp { get; set; }
		public DateTime PlatformTimestamp { get; set; }

		[JsonRequired]
		public DataPointMap Data { get; set; }

		[JsonIgnore]
		public ObjectId SensorID => this.SensorId;

		[JsonIgnore]
		public MessageType Type => MessageType.Measurement;
	}
}
