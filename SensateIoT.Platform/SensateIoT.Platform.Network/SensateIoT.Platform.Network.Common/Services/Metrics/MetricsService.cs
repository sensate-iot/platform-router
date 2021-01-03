/*
 * Metrics server implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Prometheus;

using SensateIoT.Platform.Network.Common.Settings;

using BackgroundService = SensateIoT.Platform.Network.Common.Services.Background.BackgroundService;

namespace SensateIoT.Platform.Network.Common.Services.Metrics
{
	public class MetricsService : BackgroundService
	{
		private readonly MetricServer m_server;

		public MetricsService(IOptions<MetricsOptions> options)
		{
			var hostname = options.Value.Hostname;

			if(hostname == "0.0.0.0" || string.IsNullOrEmpty(hostname)) {
				hostname = "+";
			}

			this.m_server = new MetricServer(hostname, options.Value.Port, options.Value.Endpoint);
		}

		public override Task ExecuteAsync(CancellationToken token)
		{
			this.m_server.Start();
			return Task.CompletedTask;
		}
	}
}