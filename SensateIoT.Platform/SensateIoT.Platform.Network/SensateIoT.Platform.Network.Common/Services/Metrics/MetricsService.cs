/*
 * Metrics server implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Prometheus;

using SensateIoT.Platform.Network.Common.Settings;

using BackgroundService = SensateIoT.Platform.Network.Common.Services.Background.BackgroundService;

namespace SensateIoT.Platform.Network.Common.Services.Metrics
{
	public class MetricsService : BackgroundService
	{
		private readonly MetricServer m_server;
		private readonly ILogger<MetricsService> m_logger;
		private readonly MetricsOptions m_options;

		public MetricsService(IOptions<MetricsOptions> options, ILogger<MetricsService> logger)
		{
			var hostname = options.Value.Hostname;

			if(hostname == "0.0.0.0" || string.IsNullOrEmpty(hostname)) {
				hostname = "+";
			}

			options.Value.Hostname = hostname;

			if(string.IsNullOrEmpty(options.Value.Endpoint)) {
				options.Value.Endpoint = "metrics/";
			}

			if(options.Value.Port == default) {
				options.Value.Port = 8080;
			}

			this.m_options = options.Value;
			this.m_server = new MetricServer(hostname, options.Value.Port, options.Value.Endpoint);
			this.m_logger = logger;
		}

		public override Task ExecuteAsync(CancellationToken token)
		{
			this.m_logger.LogInformation("Starting metrics server on http://{hostname}:{port}/{endpoint}",
										 this.m_options.Hostname,
										 this.m_options.Port,
										 this.m_options.Endpoint);
			this.m_server.Start();
			return Task.CompletedTask;
		}
	}
}