/*
 * Control message model for actuators.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.ComponentModel.DataAnnotations;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

using SensateService.Converters;

namespace SensateService.Models
{
	public class ControlMessage
	{
        [BsonId, BsonRequired]
		public ObjectId InternalId { get; set; }
        public int NodeId { get; set; }
        [BsonRequired, Required, JsonConverter(typeof(ObjectIdJsonConverter))]
        public ObjectId SensorId { get; set; }
        [BsonRequired, Required]
        public string Data { get; set; }
        public DateTime Timestamp { get; set; }
	}
}