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
using Newtonsoft.Json.Linq;
using SensateService.Converters;

namespace SensateService.Models
{
	public class Measurement
	{
		[BsonId, BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId InternalId {get;set;}
		[BsonRequired, JsonConverter(typeof(BsonDocumentConverter))]
		public BsonDocument Data {get;set;}
		public double Longitude {get;set;}
		public double Latitude {get;set;}
		[BsonRequired]
		public DateTime CreatedAt {get;set;}
		[BsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId CreatedBy {get;set;}

		public Measurement()
		{
			this.InternalId = ObjectId.Empty;
			this.CreatedBy = ObjectId.Empty;
		}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}

		public T ConvertData<T>()
		{
			string json = this.Data.ToJson(BsonDocumentConverter.JsonWriterSettings);
			return JsonConvert.DeserializeObject<T>(json);
		}
	}
}
