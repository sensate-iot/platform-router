/*
 * Measurement model
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SensateService.Common.Data.Converters;

namespace SensateService.Common.Data.Models
{
	using DataPointMap = IDictionary<string, DataPoint>;

	[Serializable]
	public class Measurement
	{
		[BsonRequired]
		public DataPointMap Data { get; set; }
		[BsonRequired]
		public DateTime Timestamp { get; set; }
		[BsonRequired]
		public DateTime PlatformTime { get; set; }
		[JsonConverter(typeof(GeoJsonPointJsonConverter))]
		public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}

		public static bool TryParseData(JToken data, out DataPointMap output)
		{
			DataPointMap datapoints;

			if(data == null) {
				output = null;
				return false;
			}

			try {
				datapoints = data.ToObject<DataPointMap>();
			} catch(JsonSerializationException) {
				output = null;
				return false;
			}

			output = datapoints;
			return true;
		}
	}
}
