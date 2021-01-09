using System;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SensateIoT.Platform.Ingress.DataAccess.Models
{
	public class Sensor
	{
		[BsonId]
		public ObjectId Id { get; set; }
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