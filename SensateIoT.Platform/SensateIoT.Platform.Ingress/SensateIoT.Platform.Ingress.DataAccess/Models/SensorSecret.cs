using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SensateIoT.Platform.Ingress.DataAccess.Models
{
	public class SensorSecret
	{
		[BsonId]
		public ObjectId Id { get; set; }
		[BsonRequired]
		public string Secret { get; set; }
	}
}