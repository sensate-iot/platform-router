/*
 * Data message model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using SensateService.Converters;

namespace SensateService.Models
{
	public class Message
	{
		[BsonId, BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId InternalId { get; set; }
		[BsonRequired]
		public DateTime CreatedAt { get; set; }
		[BsonRequired, JsonIgnore]
		public DateTime UpdatedAt { get; set; }
		[BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId SensorId { get; set; }
		[BsonRequired, StringLength(1024, MinimumLength = 1)]
		public string Data { get; set; }
	}
}
