/*
 * Sensor model
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.ComponentModel.DataAnnotations;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

using SensateService.Converters;

namespace SensateService.Models
{
	public class Sensor
	{
		public const int SecretLength = 256;

		[BsonId, BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId InternalId { get; set; }
		[BsonRequired, StringLength(SecretLength, MinimumLength = 4)]
		public string Secret { get; set; }
		[BsonRequired, Required]
		public string Name { get; set; }
		[Required]
		public string Description { get; set; }
		[BsonRequired]
		public DateTime CreatedAt { get; set; }
		[BsonRequired]
		public DateTime UpdatedAt { get; set; }
		[BsonRequired]
		public string Owner { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
