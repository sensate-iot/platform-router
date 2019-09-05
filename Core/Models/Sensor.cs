/*
 * Sensor model
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SensateService.Converters;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace SensateService.Models
{
	public class Sensor
	{
		public const int SecretLength = 256;

		[BsonId, BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId InternalId {get;set;}
		[BsonRequired, StringLength(256, MinimumLength = 4)]
		public string Secret {get;set;}
		[BsonRequired]
		public string Name {get;set;}
		public string Description {get;set;}
		[BsonRequired]
		public DateTime CreatedAt {get;set;}
		[BsonRequired]
		public DateTime UpdatedAt {get;set;}
		[BsonRequired]
		public string Owner {get;set;}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
