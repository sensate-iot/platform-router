/*
 * Database message model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.ComponentModel.DataAnnotations;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json;

using SensateIoT.Platform.Network.Data.Converters;

namespace SensateIoT.Platform.Network.Data.Models
{
	public class Message
	{
		[BsonId, BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId InternalId { get; set; }
		[BsonRequired]
		public DateTime Timestamp { get; set; }
		[BsonRequired]
		public DateTime PlatformTimestamp { get; set; }
		[BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId SensorId { get; set; }
		[JsonConverter(typeof(GeoJsonPointJsonConverter))]
		public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; }
		[BsonRequired, StringLength(8192, MinimumLength = 1)]
		public string Data { get; set; }
	}
}
