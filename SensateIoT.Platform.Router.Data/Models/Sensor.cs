/*
 * Sensor database model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

using JetBrains.Annotations;
using Newtonsoft.Json;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using SensateIoT.Platform.Router.Data.Converters;

namespace SensateIoT.Platform.Router.Data.Models
{
	[PublicAPI]
	public class Sensor
	{
		[BsonId, BsonRequired, JsonProperty("id"), JsonConverter(typeof(ObjectIdJsonConverter))]
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
