/*
 * Measurement model
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

using MongoDB.Bson.Serialization.Attributes;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SensateService.Converters;

namespace SensateService.Models
{
	[Serializable]
	[BsonSerializer(typeof(BsonMeasurementSerializer))]
	public class Measurement : ISerializable
	{
		[BsonRequired]
		public IEnumerable<DataPoint> Data { get;set; }
		[BsonRequired]
		public double Longitude {get;set;}
		[BsonRequired]
		public double Latitude {get;set;}
		[BsonRequired]
		public DateTime CreatedAt {get;set;}

		public Measurement()
		{
		}

		protected Measurement(SerializationInfo info, StreamingContext context)
		{
			this.Data = info.GetValue("Data", typeof(IEnumerable<DataPoint>)) as IEnumerable<DataPoint>;
			this.Longitude = info.GetDouble("Longitude");
			this.Latitude = info.GetDouble("Latitude");
			this.CreatedAt = info.GetDateTime("CreatedAt");
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
			info.AddValue("Data", this.Data, typeof(IEnumerable<DataPoint>));
			info.AddValue("Longitude", this.Longitude);
			info.AddValue("Latitude", this.Latitude);
			info.AddValue("CreatedAt", this.CreatedAt);
		}
	}
}
