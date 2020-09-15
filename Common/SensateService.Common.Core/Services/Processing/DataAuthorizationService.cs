/*
 * Data authorization service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateService.Common.Config.Settings;
using SensateService.Helpers;
using SensateService.Infrastructure.Authorization.Cache;
using SensateService.Services.Background;

namespace SensateService.Services.Processing
{
	public class DataAuthorizationService : TimedBackgroundService
	{
		private const int Interval = 1000;
		private const int StartDelay = 1000;

		private readonly IAuthorizationCache m_cache;
		private readonly ILogger<DataAuthorizationService> m_logger;
		private readonly TimeSpan m_reloadInterval;
		private DateTimeOffset m_reloadExpiry;

		public DataAuthorizationService(IAuthorizationCache cache, ILogger<DataAuthorizationService> logger)
		{
			this.m_cache = cache;
			this.m_logger = logger;
			this.m_reloadExpiry = DateTimeOffset.MinValue;
			this.m_reloadInterval = TimeSpan.FromMinutes(5);
		}

		protected override async Task ProcessAsync()
		{
			Stopwatch sw;
			long count, authorized;

			sw = Stopwatch.StartNew();
			this.m_logger.LogDebug("Authorization service triggered!");
			count = 0L;
			authorized = 0L;

			try {
				if(DateTimeOffset.UtcNow > this.m_reloadExpiry) {
					this.m_logger.LogInformation("Reloading caches.");
					this.m_reloadExpiry = DateTimeOffset.UtcNow.Add(this.m_reloadInterval);
					await this.m_cache.Load().AwaitBackground();
				}

				var tmp = await this.m_cache.ProcessAsync().AwaitBackground();
				count = tmp.Item1;
				authorized = tmp.Item2;

				await this.m_cache.ProcessCommandsAsync().AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogWarning(ex, $"Authorization cache failed: {ex.InnerException?.Message}");
			}

			sw.Stop();

			if(count > 0 || authorized > 0) {
				this.m_logger.LogInformation("Number of messages processed: {processed}" + Environment.NewLine +
											 "Number of messages authorized: {authorized}" + Environment.NewLine +
											 "Processing took {duration}ms.", count, authorized, sw.ElapsedMilliseconds);
			}
		}

		protected override void Configure(TimedBackgroundServiceSettings settings)
		{
			settings.Interval = Interval;
			settings.StartDelay = StartDelay;
		}
	}
}