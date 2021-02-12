/*
 * Measurement DTO object.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using MongoDB.Bson;
using Newtonsoft.Json;
using SensateIoT.Platform.Network.Data.Converters;

using DataPointMap = System.Collections.Generic.IDictionary<string, SensateIoT.Platform.Network.Data.Models.DataPoint>;

namespace SensateIoT.Platform.Network.API.DTO
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