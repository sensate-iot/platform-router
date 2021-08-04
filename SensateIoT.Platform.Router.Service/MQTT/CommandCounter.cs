using Prometheus;

namespace SensateIoT.Platform.Router.Service.MQTT
{
	public class CommandCounter
	{
		public Counter Counter { get; }

		public CommandCounter()
		{
			this.Counter = Metrics.CreateCounter("router_commands_total", "Total amount of received commands.");
		}
	}
}
