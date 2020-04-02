/*
 * JSON model for counting query's.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateService.DataApi.Json
{
	public class Count
	{
		public long Measurements { get; set; }
		public long Messages { get; set; }
		public long Sensors { get; set; }
		public long Links { get; set; }
		public long TriggerInvocations { get; set; }
		public long ApiCalls { get; set; }
	}
}
