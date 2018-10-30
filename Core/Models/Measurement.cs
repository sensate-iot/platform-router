/*
 * Measurement model
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
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
		public ObjectId CreatedBy { get; set; }

		public Measurement()
		{
			this.InternalId = ObjectId.Empty;
			this.CreatedBy = ObjectId.Empty;
		}

		protected Measurement(SerializationInfo info, StreamingContext context)
		{
			this.InternalId = ObjectId.Parse(info.GetString("InternalId"));
			this.Data = info.GetValue("Data", typeof(IEnumerable<DataPoint>)) as IEnumerable<DataPoint>;
			this.Longitude = info.GetDouble("Longitude");
			this.Latitude = info.GetDouble("Latitude");
			this.CreatedAt = info.GetDateTime("CreatedAt");
			this.CreatedBy = ObjectId.Parse(info.GetString("CreatedBy"));
		}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}

		public static Measurement FromJson(string json)
		{
			Measurement obj = null;

			try {
				obj = JsonConvert.DeserializeObject<Measurement>(json);
			} catch(JsonSerializationException ex) {
				Debug.WriteLine(ex.Message);
				obj = null;
			}

			return obj;
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
			info.AddValue("InternalId", this.InternalId.ToString());
			info.AddValue("Data", this.Data, typeof(IEnumerable<DataPoint>));
			info.AddValue("Longitude", this.Longitude);
			info.AddValue("Latitude", this.Latitude);
			info.AddValue("CreatedAt", this.CreatedAt);
			info.AddValue("CreatedBy", this.CreatedBy.ToString());
		}
	}
}
