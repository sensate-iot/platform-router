/*
 * JSON model for counting query's.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateService.Api.DataApi.Json
{
	public class Count
	{
		public long Measurements { get; set; }
		public long Messages { get; set; }
		public long Sensors { get; set; }
		public long Links { get; set; }
		public long TriggerInvocations { get; set; }
		public long ApiCalls { get; set; }
		public long BlobStorage { get; set; }
		public long ActuatorMessages { get; set; }
	}
}
