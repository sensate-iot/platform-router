/*
 * Measurement geo query result model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;

using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json;

using SensateService.Converters;

namespace SensateService.Models.Generic
{
	public class MeasurementsQueryResult
	{
		[JsonIgnore]
		public ObjectId _id { get; set; }
		public DateTime Timestamp { get; set; }
		[JsonConverter(typeof(GeoJsonPointJsonConverter))]
		public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; }
		public IDictionary<string, DataPoint> Data { get;set; }
	}
}