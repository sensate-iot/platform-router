/*
 * Authorizaiton proxy service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using SensateService.Config;
using SensateService.Helpers;
using SensateService.Infrastructure.Authorization;
using SensateService.Services.Settings;

namespace SensateService.Services.Processing
{
	public class AuthProxyService : TimedBackgroundService
	{
		public const int Interval = 1000;
		private const int StartDelay = 200;

		private readonly ILogger<CacheService> _logger;
		private readonly IMeasurementAuthorizationProxyCache m_measurementProxy;
		private readonly IMessageAuthorizationProxyCache m_messageProxy;
		private readonly SystemConfig m_config;

		private long _totalCount;

		public AuthProxyService(ILogger<CacheService> logger,
		                        IMeasurementAuthorizationProxyCache measurements,
		                        IMessageAuthorizationProxyCache messages,
								SystemConfig config
			)
		{
			this._logger = logger;
			this.m_measurementProxy = measurements;
			this.m_messageProxy = messages;
			this._totalCount = 0L;
			this.m_config = config;
		}

		protected override async Task ProcessAsync()
		{
			Stopwatch sw;
			long count;
			long totaltime;

			sw = Stopwatch.StartNew();
			this._logger.LogTrace("Authorization service triggered!");
			count = 0L;

			try {
				var result = await Task.WhenAll(
					this.m_measurementProxy.ProcessAsync(this.m_config.MeasurementAuthProxyUrl),
					this.m_messageProxy.ProcessAsync(this.m_config.MessageAuthProxyUrl)
				).AwaitBackground();

				count = result.Sum();
			} catch(Exception ex) {
				this._logger.LogInformation($"Authorization cache failed: {ex.InnerException?.Message}");
			}

			sw.Stop();

			totaltime = this.MillisecondsElapsed();
			this._totalCount += count;

			if(count > 0) {
				this._logger.LogInformation($"Number of messages proxied: {count}.{Environment.NewLine}" +
											$"Processing took {sw.ElapsedMilliseconds}ms.{Environment.NewLine}" +
											$"Average messages per second: {this._totalCount / (totaltime / 1000)}.");
			}
		}

		protected override void Configure(TimedBackgroundServiceSettings settings)
		{
			settings.Interval = Interval;
			settings.StartDelay = StartDelay;
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			await base.StopAsync(cancellationToken).AwaitBackground();
			this._logger.LogInformation("Stopping proxy cache service!");
		}
	}
}