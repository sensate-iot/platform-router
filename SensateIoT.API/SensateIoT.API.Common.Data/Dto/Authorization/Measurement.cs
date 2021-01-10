/*
 * Measurement DTO object.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */


using System;
using MongoDB.Bson;
using Newtonsoft.Json;
using SensateIoT.API.Common.Data.Converters;
using DataPointMap = System.Collections.Generic.IDictionary<string, SensateIoT.API.Common.Data.Models.DataPoint>;

namespace SensateIoT.API.Common.Data.Dto.Authorization
{
	public class Measurement
	{
		[JsonRequired, JsonConverter(typeof(ObjectIdJsonConverter))]
		public ObjectId SensorId { get; set; }
		[JsonRequired]
		public string Secret { get; set; }
		public decimal Longitude { get; set; }
		public decimal Latitude { get; set; }
		public DateTime Timestamp { get; set; }
		[JsonRequired]
		public DataPointMap Data { get; set; }
	}
}
