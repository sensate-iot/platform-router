/*
 * Sensor model
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SensateService.Models
{
	public class Sensor
	{
		[BsonId, BsonRequired]
		public ObjectId InternalId {get;set;}
		[BsonRequired, StringLength(128, MinimumLength = 128)]
		public string Secret {get;set;}
		[BsonRequired, Required]
		public string Name {get;set;}
		public string Unit {get;set;}
		[BsonRequired]
		public DateTime CreatedAt {get;set;}
		[BsonRequired]
		public DateTime UpdatedAt {get;set;}
	}
}
