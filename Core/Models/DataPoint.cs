/*
 * Time series datapoint.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using Newtonsoft.Json;
using SensateService.Converters;

using MongoDB.Bson.Serialization.Attributes;

namespace SensateService.Models
{
	[Serializable]
	public class DataPoint
	{
		public string Unit { get; set; }
		[JsonConverter(typeof(DecimalJsonConverter))]
		[JsonProperty(Required = Required.Always)]
		[BsonRequired]
		public decimal Value { get; set; }

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
