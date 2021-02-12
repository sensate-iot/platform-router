/*
 * Live data refresh service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Services.Background;
using SensateIoT.Platform.Network.Common.Settings;

namespace SensateIoT.Platform.Network.Common.Services.Data
{
	public class LiveDataReloadService : TimedBackgroundService
	{
		private readonly ILogger<LiveDataReloadService> m_logger;
		private readonly IDataCache m_cache;

		public LiveDataReloadService(IDataCache cache,
									 IOptions<DataReloadSettings> settings,
									 ILogger<LiveDataReloadService> logger) : base(TimeSpan.FromSeconds(5), settings.Value.LiveDataReloadInterval)
		{
			this.m_logger = logger;
			this.m_cache = cache;
		}

		public override Task ExecuteAsync(CancellationToken token)
		{
			this.m_logger.LogInformation("Flushing live data routes.");
			this.m_cache.FlushLiveData();
			this.m_logger.LogInformation("Finished flushing live data routes.");
			return Task.CompletedTask;
		}
	}
}
