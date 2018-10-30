/*
 * Time series datapoint.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
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
		[JsonProperty(Required = Required.Always)]
		[BsonRequired]
		public string Name { get; set; }
		public string Unit { get; set; }
		[JsonConverter(typeof(DecimalJsonConverter))]
		[JsonProperty(Required = Required.Always)]
		[BsonRequired]
		public decimal Value { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
