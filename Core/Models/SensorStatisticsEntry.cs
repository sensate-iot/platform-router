/*
 * Statistics entry for a single day for a specific sensor.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

using SensateService.Converters;
using SensateService.Enums;

namespace SensateService.Models
{
	public class SensorStatisticsEntry
	{
		[BsonId, BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId InternalId { get; set; }
		[BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId SensorId { get; set; }
		[BsonRequired]
		public DateTime Date { get; set; }
		[BsonRequired]
		public int Measurements { get; set; }
		public RequestMethod Method { get; set; }
	}
}