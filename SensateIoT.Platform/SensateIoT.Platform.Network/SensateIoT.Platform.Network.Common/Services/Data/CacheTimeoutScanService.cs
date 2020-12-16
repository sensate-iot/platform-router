/*
 * Scan the data cache for timeouts.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SensateIoT.Platform.Network.Common.Caching.Object;
using SensateIoT.Platform.Network.Common.Services.Background;
using SensateIoT.Platform.Network.Common.Settings;

namespace SensateIoT.Platform.Network.Common.Services.Data
{
	public class CacheTimeoutScanService : TimedBackgroundService
	{
		private readonly IDataCache m_cache;
		private readonly ILogger<CacheTimeoutScanService> m_logger;

		public CacheTimeoutScanService(IDataCache cache, ILogger<CacheTimeoutScanService> logger, IOptions<DataReloadSettings> settings) :
			base(settings.Value.TimeoutScanInterval, settings.Value.TimeoutScanInterval)
		{
			this.m_cache = cache;
			this.m_logger = logger;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			this.m_logger.LogInformation("Starting cache clean up!");

			var sw = Stopwatch.StartNew();
			await this.m_cache.ScanCachesAsync().ConfigureAwait(false);
			sw.Stop();

			this.m_logger.LogInformation("Finished sensor timeout scan in {milliseconds}ms.", sw.ElapsedMilliseconds);
		}
	}
}
