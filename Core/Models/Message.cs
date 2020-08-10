﻿/*
 * Data message model.
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

using SensateService.Converters;

namespace SensateService.Models
{
	public class Message
	{
		[BsonId, BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId InternalId { get; set; }
		[BsonRequired]
		public DateTime Timestamp { get; set; }
		[BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId SensorId { get; set; }
		[JsonConverter(typeof(GeoJsonPointJsonConverter))]
		public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; }
		[BsonRequired, StringLength(1024, MinimumLength = 1)]
		public string Data { get; set; }
	}
}
