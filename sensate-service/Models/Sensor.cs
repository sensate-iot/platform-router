/*
 * Sensor model
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Linq;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SensateService.Models
{
	public class Sensor
	{
		[BsonId]
		public ObjectId InternalId {get;set;}
		public string Secret {get;set;}
		public string Name {get;set;}
		public string Unit {get;set;}
		public DateTime CreatedAt {get;set;}
		public DateTime UpdatedAt {get;set;}
	}
}
