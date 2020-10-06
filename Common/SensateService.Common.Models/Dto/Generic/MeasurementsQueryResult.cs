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

using SensateService.Common.Data.Converters;
using SensateService.Common.Data.Models;

namespace SensateService.Common.Data.Dto.Generic
{
	public class MeasurementsQueryResult
	{
		[JsonIgnore]
		public ObjectId _id { get; set; }
		public ObjectId SensorId { get; set; }
		public DateTime Timestamp { get; set; }
		[JsonConverter(typeof(GeoJsonPointJsonConverter))]
		public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; }
		public IDictionary<string, DataPoint> Data { get; set; }
	}
}