/*
 * Time series datapoint.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;

using MongoDB.Bson.Serialization.Attributes;

using Newtonsoft.Json;

using SensateIoT.Platform.Network.Data.Converters;

namespace SensateIoT.Platform.Network.Data.Models
{
	[Serializable]
	public class DataPoint
	{
		public string Unit { get; set; }
		[JsonConverter(typeof(DecimalJsonConverter))]
		[JsonProperty(Required = Required.Always)]
		[BsonRequired]
		public decimal Value { get; set; }
		[BsonIgnoreIfNull]
		public double? Precision { get; set; }
		[BsonIgnoreIfNull]
		public double? Accuracy { get; set; }

		public DataPoint()
		{ }

		public DataPoint(decimal value, string unit = null)
		{
			this.Value = value;
			this.Unit = unit;
		}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
