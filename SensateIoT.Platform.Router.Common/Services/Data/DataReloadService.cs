﻿/*
 * Periodically reload database data.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SensateIoT.Platform.Router.Common.Caching.Abstract;
using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Services.Background;
using SensateIoT.Platform.Router.Common.Settings;
using SensateIoT.Platform.Router.Data.DTO;
using SensateIoT.Platform.Router.DataAccess.Abstract;

namespace SensateIoT.Platform.Router.Common.Services.Data
{
	public class DataReloadService : TimedBackgroundService
	{
		private readonly IServiceProvider m_provider;
		private readonly IRemoteLiveDataQueue m_queue;
		private readonly IRoutingCache m_cache;
		private readonly ILogger<DataReloadService> m_logger;
		private bool m_loaded;
		private readonly DataReloadSettings m_settings;

		public DataReloadService(IServiceProvider provider,
											IRemoteLiveDataQueue liveDataQueue,
											IRoutingCache cache,
											IOptions<DataReloadSettings> settings,
											ILogger<DataReloadService> logger) : base(settings.Value.StartDelay, settings.Value.DataReloadInterval, logger)
		{
			this.m_provider = provider;
			this.m_queue = liveDataQueue;
			this.m_logger = logger;
			this.m_cache = cache;
			this.m_loaded = false;
			this.m_settings = settings.Value;
		}

		protected override async Task ExecuteAsync(CancellationToken token)
		{
			await this.ReloadLiveDataHandlers(token).ConfigureAwait(false);

			if(this.CanReload()) {
				await this.ReloadAccounts(token).ConfigureAwait(false);
				await this.ReloadSensors(token).ConfigureAwait(false);
				await this.ReloadApiKeys(token).ConfigureAwait(false);

				this.m_loaded = true;
			}
		}

		private bool CanReload()
		{
			return !this.m_loaded || this.m_settings.EnableReload;
		}

		private async Task ReloadAccounts(CancellationToken token)
		{
			this.m_logger.LogInformation("Starting account reload at {reloadStart}", DateTime.UtcNow);

			using var scope = this.m_provider.CreateScope();
			var accountRepo = scope.ServiceProvider.GetRequiredService<IRoutingRepository>();

			var sw = Stopwatch.StartNew();
			var rawAccounts = await accountRepo.GetAccountsForRoutingAsync(token).ConfigureAwait(false);
			var accounts = rawAccounts.ToList();

			this.m_logger.LogInformation("Bulk loaded {accountCount} accounts.", accounts.Count);
			this.m_cache.Load(accounts);
			sw.Stop();

			this.m_logger.LogInformation("Finished account reload at {reloadEnd}. Reload took {duration}ms.",
										 DateTime.UtcNow, sw.ElapsedMilliseconds);
		}

		private async Task ReloadApiKeys(CancellationToken token)
		{
			this.m_logger.LogInformation("Starting API key reload at {reloadStart}", DateTime.UtcNow);

			using var scope = this.m_provider.CreateScope();
			var apiKeyRepo = scope.ServiceProvider.GetRequiredService<IRoutingRepository>();

			var sw = Stopwatch.StartNew();
			var rawKeys = await apiKeyRepo.GetApiKeysAsync(token).ConfigureAwait(false);
			var keys = rawKeys.ToList();

			this.m_logger.LogInformation("Bulk loaded {keyCount} sensor keys.", keys.Count);
			this.m_cache.Load(keys);
			sw.Stop();

			this.m_logger.LogInformation("Finished API key reload at {reloadEnd}. Reload took {duration}ms.",
										 DateTime.UtcNow, sw.ElapsedMilliseconds);
		}

		private async Task ReloadLiveDataHandlers(CancellationToken token)
		{
			this.m_logger.LogInformation("Starting live data handler reload at {reloadStart}", DateTime.UtcNow);

			using var scope = this.m_provider.CreateScope();
			var handlerRepo = scope.ServiceProvider.GetRequiredService<ILiveDataHandlerRepository>();

			var sw = Stopwatch.StartNew();
			var rawHandlers = await handlerRepo.GetLiveDataHandlers(token).ConfigureAwait(false);
			var handlers = rawHandlers.ToList();
			this.m_logger.LogInformation("Bulk loaded {count} live data handlers.", handlers.Count);

			this.m_queue.SyncLiveDataHandlers(handlers);
			this.m_cache.SetLiveDataRemotes(handlers);
			sw.Stop();

			this.m_logger.LogInformation("Finished live data handler reload at {reloadEnd}. Reload took {duration}ms.",
										 DateTime.UtcNow, sw.ElapsedMilliseconds);
		}

		private async Task ReloadSensors(CancellationToken token)
		{
			this.m_logger.LogInformation("Starting sensor reload at {reloadStart}", DateTime.UtcNow);

			using var scope = this.m_provider.CreateScope();
			var routingRepo = scope.ServiceProvider.GetRequiredService<IRoutingRepository>();

			var sw = Stopwatch.StartNew();
			var sensorTask = routingRepo.GetSensorsAsync(token);
			var triggerTask = routingRepo.GetTriggerInfoAsync(token);

			var sensors = await sensorTask.ConfigureAwait(false);
			var dict = sensors.ToDictionary(k => k.ID, v => v);
			this.m_logger.LogDebug("Finished loading sensors in {ms}ms.", sw.ElapsedMilliseconds);

			var rawTriggers = await triggerTask.ConfigureAwait(false);
			var triggers = rawTriggers.ToList();
			this.m_logger.LogDebug("Finished loading trigger routes in {ms}ms.", sw.ElapsedMilliseconds);

			this.m_logger.LogInformation("Bulk loaded {sensorCount} sensors.", dict.Count);
			this.m_logger.LogInformation("Bulk loaded {triggerCount} trigger routes.", triggers.Count);

			foreach(var info in triggers) {
				var route = new SensorTrigger {
					HasActions = info.ActionCount > 0,
					IsTextTrigger = info.TextTrigger
				};

				var loaded = dict.TryGetValue(info.SensorID, out var sensor);

				if(!loaded || sensor == null) {
					this.m_logger.LogWarning("Found trigger route for a non-existing sensor. Sensor ID: {sensorID}.", info.SensorID.ToString());
					continue;
				}

				sensor.TriggerInformation ??= new List<SensorTrigger>();
				sensor.TriggerInformation.Add(route);
			}

			this.m_cache.Load(dict.Values);
			sw.Stop();
			this.m_logger.LogInformation("Finished sensor reload at {reloadEnd}. Reload took {duration}ms.", DateTime.UtcNow, sw.ElapsedMilliseconds);
		}

	}
}