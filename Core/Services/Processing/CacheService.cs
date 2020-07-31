/*
 * Measurement cache service implementation details.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateService.Config;
using SensateService.Exceptions;
using SensateService.Helpers;
using SensateService.Infrastructure.Storage;
using SensateService.Services.Settings;

namespace SensateService.Services.Processing
{
	public class CacheService : TimedBackgroundService
	{
		public const int IntervalInMillis = 2000;
		private const int StartDelay = 200;

		private readonly ILogger<CacheService> _logger;
		private readonly CacheConfig _config;

		private readonly ICachedMeasurementStore _measurementStore;
		private readonly ICachedMessageStore _messageStore;
		private long _totalCount;

		public CacheService(CacheConfig config, ILogger<CacheService> logger, ICachedMeasurementStore measurements, ICachedMessageStore msgs)
		{
			this._logger = logger;
			this._config = config;

			if(config.Interval <= 0) {
				this._config.Interval = IntervalInMillis;
			}

			this._messageStore = msgs;
			this._measurementStore = measurements;
			this._totalCount = 0L;
		}

		protected override async Task ProcessAsync()
		{
			Stopwatch sw;
			long count;
			long totaltime;

			sw = Stopwatch.StartNew();
			this._logger.LogTrace("Cache service triggered!");
			count = 0L;

			try {
				count = await this._measurementStore.ProcessMeasurementsAsync().AwaitBackground();
				await this._messageStore.ProcessMessagesAsync().AwaitBackground();
			} catch(CachingException ex) {
				this._logger.LogInformation($"Measurement cache failed: {ex.InnerException?.Message}");
			}

			sw.Stop();

			totaltime = base.MillisecondsElapsed();
			this._totalCount += count;

			if(count > 0) {
				this._logger.LogInformation($"Number of measurements processed: {count}.{Environment.NewLine}" +
											$"Processing took {sw.ElapsedMilliseconds}ms.{Environment.NewLine}" +
											$"Average measurements per second: {this._totalCount / (totaltime / 1000)}.");
			}
		}

		protected override void Configure(TimedBackgroundServiceSettings settings)
		{
			settings.Interval = this._config.Interval;
			settings.StartDelay = StartDelay;
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			await base.StopAsync(cancellationToken).AwaitBackground();

			this._measurementStore.Destroy();
			this._logger.LogInformation("Stopping measurement cache service!");
		}
	}
}
