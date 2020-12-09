/*
 * Statistics entry for a single day for a specific sensor.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using Newtonsoft.Json;

using SensateIoT.Platform.Network.Data.Converters;
using SensateIoT.Platform.Network.StorageService.DTO;

namespace SensateIoT.Platform.Network.Data.Models
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
