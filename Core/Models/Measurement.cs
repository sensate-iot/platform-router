/*
 * Measurement model
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Collections.Generic;

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
		[BsonRequired]
		public IEnumerable<DataPoint> Data { get;set; }
		[BsonRequired]
		public double Longitude {get;set;}
		[BsonRequired]
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

		public static bool TryParseData(JToken obj, out IEnumerable<DataPoint> result)
		{
			IEnumerable<DataPoint> dataPoints;

			try {
				dataPoints = obj.ToObject<IEnumerable<DataPoint>>();
			} catch(JsonSerializationException) {
				result = null;
				return false;
			}

			result = dataPoints;
			return true;
		}
	}
}
