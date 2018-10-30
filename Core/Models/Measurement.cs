/*
 * Measurement model
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SensateService.Converters;

namespace SensateService.Models
{
	[BsonSerializer(typeof(BsonMeasurementSerializer))]
	[Serializable]
	public class Measurement : ISerializable
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

		protected Measurement(SerializationInfo info, StreamingContext context)
		{
			this.InternalId = ObjectId.Parse(info.GetString("id"));
			this.Data = info.GetValue("data", typeof(IEnumerable<DataPoint>)) as IEnumerable<DataPoint>;
			this.Longitude = info.GetDouble("lon");
			this.Latitude = info.GetDouble("lat");
			this.CreatedAt = info.GetDateTime("createdat");
			this.CreatedBy = ObjectId.Parse(info.GetString("createdby"));
		}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}

		public static bool TryParseData(JToken data, out IEnumerable<DataPoint> output)
		{
			IEnumerable<DataPoint> datapoints;

			if(data == null) {
				output = null;
				return false;
			}

			try {
				datapoints = data.ToObject<IEnumerable<DataPoint>>();
			} catch(JsonSerializationException) {
				output = null;
				return false;
			}

			output = datapoints;
			return true;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("id", this.InternalId.ToString());
			info.AddValue("data", this.Data, typeof(IEnumerable<DataPoint>));
			info.AddValue("lon", this.Longitude);
			info.AddValue("lat", this.Latitude);
			info.AddValue("createdat", this.CreatedAt);
			info.AddValue("createdby", this.CreatedBy.ToString());
		}
	}
}
