/*
 * Measurement model
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;

using MongoDB.Driver.GeoJsonObjectModel;

using DataPoint = SensateIoT.Platform.Network.Data.DTO.DataPoint;

namespace SensateIoT.Platform.Network.TriggerService.DTO
{
	using DataPointMap = IDictionary<string, DataPoint>;

	public class SingleMeasurement
	{
		public DataPointMap Data { get; set; }
		public DateTime Timestamp { get; set; }
		public DateTime PlatformTime { get; set; }
		public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; }
	}
}
