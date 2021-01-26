using System.Collections.Generic;
using System.Linq;

using SensateIoT.API.Common.Data.Dto.Generic;

namespace SensateIoT.API.Common.Data.Converters
{
	public static class MeasurementConverter
	{
		public static MeasurementsQueryResult Convert(Models.MeasurementsQueryResult data)
		{
			return new MeasurementsQueryResult {
				Data = data.Data,
				Location = new GeoJsonPoint {
					Latitude = data.Location.Coordinates.Latitude,
					Longitude = data.Location.Coordinates.Longitude
				},
				SensorId = data.SensorId,
				Timestamp = data.Timestamp,
			};
		}

		public static IEnumerable<MeasurementsQueryResult> Convert(IEnumerable<Models.MeasurementsQueryResult> data)
		{
			return data.Select(Convert);
		}
	}
}