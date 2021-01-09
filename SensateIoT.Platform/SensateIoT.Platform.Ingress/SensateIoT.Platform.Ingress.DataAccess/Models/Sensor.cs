using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SensateIoT.Platform.Ingress.DataAccess.Models
{
	public class Sensor
	{
		[BsonId]
		public ObjectId Id { get; set; }
		public string Secret { get; set; }
	}
}