/*
 * Measurement DTO object.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */


using DataPointMap = System.Collections.Generic.IDictionary<string, SensateService.Common.Data.Models.DataPoint>;

namespace SensateService.Processing.DataAuthorizationApi.Dto
{
	public class Measurement
	{
		public string SensorId { get; set; }
		public string Secret { get; set; }
		public decimal Longitude { get; set; }
		public decimal Latitude { get; set; }
		public DataPointMap Data { get; set; }
	}
}
