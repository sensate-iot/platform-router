/*
 * Sensor meta data model
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Net;
using System.ComponentModel.DataAnnotations;

using SensateService.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using SensateService.Converters;

namespace SensateService.Models
{
	public class AuditLog
	{
		[BsonId, BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId InternalId {get;set;}
		[Required]
		public string Route { get; set; }
		[Required]
		public RequestMethod Method { get; set; }
		[Required]
		public IPAddress Address { get; set; }
		public string AuthorId { get; set; }
		public DateTime Timestamp { get; set; }
	}
}
