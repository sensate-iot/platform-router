namespace SensateIoT.Platform.Network.Router.Config
{
	public class InternalBrokerConfig
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Host { get; set; }
		public bool Ssl { get; set; }
		public short Port { get; set; }
		public string InternalCommandsTopic { get; set; }
		public string InternalBulkMeasurementTopic { get; set; }
		public string InternalBulkMessageTopic { get; set; }
	}
}