/*
 * Sensor database model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using Newtonsoft.Json;

namespace SensateIoT.Platform.Network.Data.Models
{
	public class Sensor
	{
		public const int SecretLength = 256;

		[BsonId, BsonRequired, JsonProperty("id")]
		public ObjectId InternalId { get; set; }

		[BsonRequired]
		public string Secret { get; set; }

		[BsonRequired]
		public string Name { get; set; }

		public string Description { get; set; }
		[BsonDefaultValue(true)]
		public bool StorageEnabled { get; set; }

		[BsonRequired]
		public DateTime CreatedAt { get; set; }

		[BsonRequired]
		public DateTime UpdatedAt { get; set; }

		[BsonRequired]
		public string Owner { get; set; }
	}
}
