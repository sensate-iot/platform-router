/*
 * Reload service for API keys.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SensateIoT.Platform.Network.Common.Caching.Object;
using SensateIoT.Platform.Network.Common.Services.Background;
using SensateIoT.Platform.Network.Common.Settings;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Repositories;

namespace SensateIoT.Platform.Network.Common.Services.Data
{
	public class ApiKeyReloadService : TimedBackgroundService
	{
		private readonly IServiceProvider m_provider;
		private readonly IDataCache m_cache;
		private readonly ILogger<ApiKeyReloadService> m_logger;

		public ApiKeyReloadService(IServiceProvider provider,
								   IDataCache cache,
								   IOptions<DataReloadSettings> settings,
								   ILogger<ApiKeyReloadService> logger) : base(settings.Value.StartDelay, settings.Value.DataReloadInterval)
		{
			this.m_provider = provider;
			this.m_cache = cache;
			this.m_logger = logger;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			this.m_logger.LogInformation("Starting API key reload at {reloadStart}", DateTime.UtcNow);

			using var scope = this.m_provider.CreateScope();
			var apiKeyRepo = scope.ServiceProvider.GetRequiredService<IRoutingRepository>();

			var sw = Stopwatch.StartNew();
			var keys = await apiKeyRepo.GetApiKeysAsync(token).ConfigureAwait(false);
			this.m_cache.Append(keys);
			sw.Stop();

			this.m_logger.LogInformation("Finished API key reload at {reloadEnd}. Reload took {duration}ms.",
										 DateTime.UtcNow, sw.ElapsedMilliseconds);
		}
	}
}