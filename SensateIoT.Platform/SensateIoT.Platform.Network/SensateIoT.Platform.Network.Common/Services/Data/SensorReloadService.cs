/*
 * Reload service for accounts.
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
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.DataAccess.Repositories;

namespace SensateIoT.Platform.Network.Common.Services.Data
{
	public class SensorReloadService : TimedBackgroundService
	{
		private readonly IServiceProvider m_provider;
		private readonly IDataCache m_cache;
		private readonly ILogger<SensorReloadService> m_logger;

		public SensorReloadService(IServiceProvider provider,
								   IDataCache cache,
								   IOptions<DataReloadSettings> settings,
								   ILogger<SensorReloadService> logger) : base(settings.Value.StartDelay, settings.Value.ReloadInterval)
		{
			this.m_provider = provider;
			this.m_cache = cache;
			this.m_logger = logger;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			this.m_logger.LogInformation("Starting sensor reload at {reloadStart}", DateTime.UtcNow);

			using var scope = this.m_provider.CreateScope();
			var sensorRepo = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
			var triggerRepo = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();

			var sw = Stopwatch.StartNew();
			var sensorTask = sensorRepo.GetSensorsAsync(token);
			var triggerTask = triggerRepo.GetTriggerInfoAsync(token);

			await Task.WhenAll(sensorTask, triggerTask).ConfigureAwait(false);
			var dict = sensorTask.Result.ToDictionary(k => k.ID, v => v);

			foreach(var info in triggerTask.Result) {
				var route = new SensorTrigger {
					HasActions = info.ActionCount > 0,
					IsTextTrigger = info.TextTrigger
				};

				dict[info.SensorID].TriggerInformation = route;
			}

			this.m_cache.Append(dict.Values);
			sw.Stop();
			this.m_logger.LogInformation("Finished sensor reload at {reloadEnd}. Reload took {duration}ms.", DateTime.UtcNow, sw.ElapsedMilliseconds);
		}
	}
}
