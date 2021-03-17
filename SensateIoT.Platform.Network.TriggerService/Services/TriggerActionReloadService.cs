using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SensateIoT.Platform.Network.Common.Services.Background;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.TriggerService.Abstract;
using SensateIoT.Platform.Network.TriggerService.Settings;

namespace SensateIoT.Platform.Network.TriggerService.Services
{
	public class TriggerActionReloadService : TimedBackgroundService
	{
		private readonly ILogger<TriggerActionReloadService> m_logger;
		private readonly IServiceProvider m_provider;
		private readonly ITriggerActionCache m_cache;

		public TriggerActionReloadService(IOptions<ReloadSettings> settings,
										  IServiceProvider provider,
										  ITriggerActionCache cache,
										  ILogger<TriggerActionReloadService> logger) :
			base(settings.Value.StartDelay, settings.Value.Interval)
		{
			this.m_logger = logger;
			this.m_cache = cache;
			this.m_provider = provider;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			this.m_logger.LogInformation("Loading trigger actions from database.");
			var sw = Stopwatch.StartNew();

			using var scope = this.m_provider.CreateScope();
			var repo = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();

			var actions = await repo.GetTriggerServiceActions(token).ConfigureAwait(false);
			this.m_cache.Load(actions);

			sw.Stop();
			this.m_logger.LogInformation("Finished loading trigger actions from database. Duration: {duration:c}.", sw.Elapsed);
		}
	}
}