namespace SensateIoT.Platform.Router.Common.Settings
{
	public class HealthCheckSettings
	{
		public int DefaultQueueLimit { get; set; }
		public int? LiveDataServiceQueueLimit { get; set; }
		public int? TriggerServiceQueueLimit { get; set; }
	}
}
