/*
 * MQTT configuration.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using JetBrains.Annotations;

namespace SensateIoT.Platform.Network.StorageService.Config
{
	[UsedImplicitly]
	public class InternalBrokerConfig
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Host { get; set; }
		public bool Ssl { get; set; }
		public short Port { get; set; }
		public string BulkMeasurementTopic { get; set; }
		public string BulkMessageTopic { get; set; }
		public string TriggerEventQueueTopic { get; set; }
		public string NetworkEventQueueTopic { get; set; }
	}
}
