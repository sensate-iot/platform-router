namespace SensateIoT.Platform.Network.Common.Settings
{
	public class InternalBrokerSettings
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public bool Ssl { get; set; }
		public string Id { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string InternalBulkMeasurementTopic { get; set; }
		public string InternalBulkMessageTopic { get; set; }
		public string TopicShare { get; set; }
	}
}