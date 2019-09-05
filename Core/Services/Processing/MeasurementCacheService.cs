/*
 * Measurement cache service implementation details.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateService.Config;
using SensateService.Helpers;
using SensateService.Infrastructure.Storage;
using SensateService.Services.Settings;

namespace SensateService.Services.Processing
{
	public class MeasurementCacheService : TimedBackgroundService 
	{
		public const int IntervalInMillis = 2000;
		private const int StartDelay = 200;

		private readonly ILogger<MeasurementCacheService> _logger;
		private readonly CacheConfig _config;

		private readonly ICachedMeasurementStore _store;
		private long _totalCount;

		public MeasurementCacheService(CacheConfig config, ILogger<MeasurementCacheService> logger, ICachedMeasurementStore store)
		{
			this._logger = logger;
			this._config = config;

			if(config.Interval <= 0)
				this._config.Interval = IntervalInMillis;

			this._store = store;
			this._totalCount = 0L;
		}

		protected override async Task ProcessAsync()
		{
			Stopwatch sw;
			long count;
			long totaltime;

			sw = Stopwatch.StartNew();
			this._logger.LogTrace("Cache service triggered!");
			count = await this._store.ProcessAsync().AwaitBackground();
			sw.Stop();

			totaltime = base.MillisecondsElapsed();
			this._totalCount += count;

			if(count > 0) {
				this._logger.LogInformation($"Number of measurements processed: {count}.{Environment.NewLine}" +
				                            $"Processing took {sw.ElapsedMilliseconds}ms.{Environment.NewLine}" +
				                            $"Average measurements per second: {this._totalCount/(totaltime/1000)}.");
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

			this._store.Destroy();
			this._logger.LogInformation("Stopping measurement cache service!");
		}
	}
}
