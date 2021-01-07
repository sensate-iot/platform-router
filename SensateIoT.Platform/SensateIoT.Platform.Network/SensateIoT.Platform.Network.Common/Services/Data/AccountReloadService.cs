/*
 * Reloading service for accounts.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SensateIoT.Platform.Network.Common.Caching.Object;
using SensateIoT.Platform.Network.Common.Services.Background;
using SensateIoT.Platform.Network.Common.Settings;
using SensateIoT.Platform.Network.DataAccess.Abstract;

namespace SensateIoT.Platform.Network.Common.Services.Data
{
	public class AccountReloadService : TimedBackgroundService
	{
		private readonly IServiceProvider m_provider;
		private readonly IDataCache m_cache;
		private readonly ILogger<AccountReloadService> m_logger;

		public AccountReloadService(IServiceProvider provider,
									IDataCache cache,
									IOptions<DataReloadSettings> settings,
									ILogger<AccountReloadService> logger) : base(settings.Value.StartDelay, settings.Value.DataReloadInterval)
		{
			this.m_provider = provider;
			this.m_cache = cache;
			this.m_logger = logger;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			this.m_logger.LogInformation("Starting account reload at {reloadStart}", DateTime.UtcNow);

			using var scope = this.m_provider.CreateScope();
			var accountRepo = scope.ServiceProvider.GetRequiredService<IRoutingRepository>();

			var sw = Stopwatch.StartNew();
			var rawAccounts = await accountRepo.GetAccountsForRoutingAsync(token).ConfigureAwait(false);
			var accounts = rawAccounts.ToList();

			this.m_logger.LogInformation("Bulk loaded {accountCount} accounts.", accounts.Count);
			this.m_cache.Append(accounts);
			sw.Stop();

			this.m_logger.LogInformation("Finished account reload at {reloadEnd}. Reload took {duration}ms.",
										 DateTime.UtcNow, sw.ElapsedMilliseconds);
		}
	}
}
