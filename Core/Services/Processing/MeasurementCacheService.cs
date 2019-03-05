/*
 * Measurement cache service implementation details.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateService.Helpers;
using SensateService.Infrastructure.Storage;
using SensateService.Services.Settings;

namespace SensateService.Services.Processing
{
	public class MeasurementCacheService : TimedBackgroundService, IMeasurementCacheService
	{
		public const int IntervalInMillis = 1000;
		private const int StartDelay = 1000;

		private readonly ILogger<MeasurementCacheService> _logger;
		private IList<ICachedMeasurementStore> _caches;
		private int _index;
		private int _count;
		private long _stored;

		public MeasurementCacheService(ILogger<MeasurementCacheService> logger)
		{
			this._logger = logger;

			this._caches = new List<ICachedMeasurementStore>();
			this._index = 0;
			this._stored = 0;
		}

		protected override async Task ProcessAsync()
		{
			Stopwatch sw;
			Task<long>[] workers;
			int count;
			long processed;
			long totaltime;
			long totalprocessed;

			sw = Stopwatch.StartNew();
			this._logger.LogTrace("Cache service triggered!");
			count = Interlocked.Add(ref this._count, 0);

			workers = new Task<long>[count];

			for(var idx = 0; idx < count; idx++) {
				var cache = this._caches[idx];
				workers[idx] = cache.ProcessAsync();
			}

			var result = await Task.WhenAll(workers).AwaitBackground();
			processed = result.Sum();
			sw.Stop();

			totalprocessed = Interlocked.Add(ref this._stored, processed);
			totaltime = base.MillisecondsElapsed();

			if(processed > 0) {
				this._logger.LogInformation($"Number of measurements processed: {processed}.{Environment.NewLine}" +
				                            $"Processing took {sw.ElapsedMilliseconds}ms.{Environment.NewLine}" +
				                            $"Average measurements per interval: {totalprocessed/(totaltime/1000)}.");
			}
		}

		protected override void Configure(TimedBackgroundServiceSettings settings)
		{
			settings.Interval = IntervalInMillis;
			settings.StartDelay = StartDelay;
		}

		public IMeasurementCache Next()
		{
			IMeasurementCache cache;
			int index, count;

			count = Interlocked.Add(ref this._count, 0);
			if(count <= 0)
				throw new IndexOutOfRangeException();

			index = Interlocked.Increment(ref this._index) - 1;
			cache = this._caches[index];
			Interlocked.CompareExchange(ref this._index, 0, count);

			return cache;
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			await base.StopAsync(cancellationToken).AwaitBackground();

			foreach(var cache in this._caches) {
				cache.Destroy();
			}

			this._logger.LogInformation("Stopping measurement cache service!");
		}

		public void RegisterCache(ICachedMeasurementStore store)
		{
			var caches = new List<ICachedMeasurementStore>(this._caches) {store};
			Interlocked.Exchange(ref this._caches, caches);
			Interlocked.Increment(ref this._count);
		}
	}
}
