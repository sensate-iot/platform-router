/*
 * Time series datapoint.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using Newtonsoft.Json;
using SensateService.Converters;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;

namespace SensateService.Models
{
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
