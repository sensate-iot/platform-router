/*
 * Validate messages.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateIoT.Platform.Router.Data.DTO;

namespace SensateIoT.Platform.Router.Common.Validators
{
	public static class MeasurementValidator
	{
		private const int DataPointCountMax = 25;

		private const int MaxLat = 90;
		private const int MinLat = -90;
		private const int MaxLon = 180;
		private const int MinLon = -180;

		public static bool Validate(Measurement measurement)
		{
			if(measurement.Data.Count > DataPointCountMax) {
				return false;
			}

			if(measurement.Latitude == 0M && measurement.Longitude == 0M) {
				return true;
			}

			return measurement.Latitude >= MinLat && measurement.Latitude <= MaxLat && measurement.Longitude >= MinLon && measurement.Longitude <= MaxLon;
		}
	}
}
