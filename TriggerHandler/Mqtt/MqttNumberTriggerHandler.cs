/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Middleware;
using SensateService.Models;
using SensateService.TriggerHandler.Models;
using SensateService.TriggerHandler.Services;
using SensateService.TriggerHandler.Utils;

namespace SensateService.TriggerHandler.Mqtt
{
	public class MqttNumberTriggerHandler : MqttHandler
	{
		private readonly IServiceProvider m_provider;
		private readonly ILogger<MqttBulkNumberTriggerHandler> m_logger;
		private readonly ITriggerNumberMatchingService m_matcher;

		public MqttNumberTriggerHandler(
			IServiceProvider provider,
			ITriggerNumberMatchingService matcher,
			ILogger<MqttBulkNumberTriggerHandler> logger
		)
		{
			this.m_provider = provider;
			this.m_logger = logger;
			this.m_matcher = matcher;
		}

		public override async Task OnMessageAsync(string topic, string message)
		{
			try {
				var measurement = JsonConvert.DeserializeObject<InternalMeasurement>(message);

				this.m_logger.LogDebug("Message received!");

				using var scope = this.m_provider.CreateScope();
				var triggersdb = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();

				var raw_triggers = await triggersdb.GetAsync(measurement.CreatedBy).AwaitBackground();
				var triggers = raw_triggers.ToList();
				var triggered = new List<Tuple<Trigger, TriggerInvocation, DataPoint>>();

				foreach(var t in triggers) {
					if(!measurement.Measurement.Data.TryGetValue(t.KeyValue, out var datapoint)) {
						continue;
					}

					if(!DataPointMatchUtility.MatchDatapoint(t, datapoint)) {
						continue;
					}

					var inv = new TriggerInvocation {
						TriggerId = t.Id,
						Timestamp = new DateTimeOffset(measurement.Measurement.Timestamp.ToUniversalTime(), TimeSpan.Zero),
					};


					triggered.Add(new Tuple<Trigger, TriggerInvocation, DataPoint>(t, inv, datapoint));
				}

				var distinct = triggered.GroupBy(t => t.Item2.TriggerId)
					.Select(g => g.First()).ToList();

				await this.m_matcher.HandleTriggerAsync(distinct).AwaitBackground();
				await triggersdb.AddInvocationsAsync(distinct.Select(t => t.Item2)).AwaitBackground();

				this.m_logger.LogDebug($"{triggered.Count} triggers triggered!");
				this.m_logger.LogDebug($"{distinct.Count} triggers handled!");
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to handle triggers {ex.Message}!");
				this.m_logger.LogDebug(ex.StackTrace);
			}
		}

		public override void OnMessage(string topic, string msg)
		{
			Task.Run(async () => { await this.OnMessageAsync(topic, msg).AwaitBackground(); }).Wait();
		}
	}
}