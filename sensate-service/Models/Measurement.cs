/*
 * Measurement model
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using Newtonsoft.Json;

namespace SensateService.Models
{
	public class Measurement
	{
		[BsonId, BsonRequired, JsonIgnore]
		public ObjectId InternalId {get;set;}
		public long Id {get;set;}
		public decimal Data {get;set;}
		public double Longitude {get;set;}
		public double Latitude {get;set;}
		public DateTime CreatedAt {get;set;}
		[JsonIgnore]
		public ObjectId CreatedBy {get;set;}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
