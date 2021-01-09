/*
 * Queue settings.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Platform.Network.Common.Settings
{
	public class QueueSettings
	{
		public string TriggerQueueTemplate { get; set; }
		public string LiveDataQueueTemplate { get; set; }
		public string MeasurementStorageQueueTopic { get; set; }
		public string MessageStorageQueueTopic { get; set; }
		public string NetworkEventQueueTopic { get; set; }
	}
}