/*
 * Measurement bucket model class.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using SensateIoT.API.Common.Data.Converters;

namespace SensateIoT.API.Common.Data.Models
{
	public class MeasurementBucket
	{
		[BsonId, BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId InternalId { get; set; }
		[BsonIgnore]
		public ObjectId _id { get; set; }
		[BsonRequired]
		public DateTime Timestamp { get; set; }
		public DateTime First { get; set; }
		public DateTime Last { get; set; }
		[BsonRequired]
		public IEnumerable<Measurement> Measurements { get; set; }
		public int Count { get; set; }
		[BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId SensorId { get; set; }
	}
}