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

using SensateService.Converters;

namespace SensateService.Models
{
	public class MeasurementBucket
	{
		[BsonId, BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId InternalId {get;set;}
		[BsonRequired]
		public DateTime Timestamp { get; set; }
		[BsonRequired]
		public ICollection<Measurement> Measurements { get; set; }
		[BsonRequired]
		public int Count { get; set; }
		[BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId SensorId { get; set; }
	}
}