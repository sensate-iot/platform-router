/*
 * Measurement geo query result model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;

using MongoDB.Bson;

using Newtonsoft.Json;

using SensateIoT.API.Common.Data.Models;

namespace SensateIoT.API.Common.Data.Dto.Generic
{
	public class MeasurementsQueryResult
	{
		[JsonIgnore]
		public ObjectId _id { get; set; }
		public ObjectId SensorId { get; set; }
		public DateTime Timestamp { get; set; }
		public GeoJsonPoint Location { get; set; }
		public IDictionary<string, DataPoint> Data { get; set; }
	}
}
