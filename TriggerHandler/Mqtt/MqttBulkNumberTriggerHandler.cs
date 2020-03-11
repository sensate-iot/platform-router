/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.TriggerHandler.Models;
using SensateService.TriggerHandler.Services;
using SensateService.TriggerHandler.Utils;

using Convert = System.Convert;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace SensateService.TriggerHandler.Mqtt
{
	public class MqttBulkNumberTriggerHandler : Middleware.MqttHandler
	{
		private readonly IServiceProvider m_provider;
		private readonly ILogger<MqttBulkNumberTriggerHandler> logger;
		private readonly ITriggerNumberMatchingService m_matcher;

		public MqttBulkNumberTriggerHandler(IServiceProvider provider,
			ITriggerNumberMatchingService matcher,
			ILogger<MqttBulkNumberTriggerHandler> logger)
		{
			this.m_provider = provider;
			this.logger = logger;
			this.m_matcher = matcher;
		}

		public override void OnMessage(string topic, string msg)
		{
			Task.Run(async () => { await this.OnMessageAsync(topic, msg).AwaitBackground(); }).Wait();
		}

		public static string Decompress(string data)
		{
			var decoded = Convert.FromBase64String(data);
			MemoryStream msi, mso;
			GZipStream gs;
			string rv;

			msi = mso = null;

			try {
				msi = new MemoryStream(decoded);
				mso = new MemoryStream();

				gs = new GZipStream(msi, CompressionMode.Decompress);
				gs.CopyTo(mso);
				gs.Dispose();

				rv = Encoding.UTF8.GetString(mso.ToArray());
			} finally {
				mso?.Dispose();
				msi?.Dispose();
			}

			return rv;
		}

		public override async Task OnMessageAsync(string topic, string message)
		{
			this.logger.LogDebug("Message received!");

			var data = Decompress(message);
			var measurements = JsonConvert.DeserializeObject<IList<InternalBulkMeasurements>>(data);

			using var scope = this.m_provider.CreateScope();
			var triggersdb = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();

			var ids = measurements.Select(m => m.CreatedBy).Distinct().ToList();

			var raw_triggers = await triggersdb.GetAsync(ids).AwaitBackground();
			var triggers = raw_triggers.ToList();
			var triggered = new List<Tuple<Trigger, TriggerInvocation, DataPoint>>();

			foreach(var mCollection in measurements) {
				var subset = triggers.Where(trigger => trigger.SensorId == mCollection.CreatedBy).ToList();

				if(subset.Count <= 0)
					continue;

				foreach(var measurement in mCollection.Measurements) {
					foreach(var t in subset) {
						if(!measurement.Data.TryGetValue(t.KeyValue, out var datapoint)) {
							continue;
						}

						if(!DataPointMatchUtility.MatchDatapoint(t, datapoint)) {
							continue;
						}

						var inv = new TriggerInvocation {
							TriggerId = t.Id,
							Timestamp = new DateTimeOffset(measurement.Timestamp.ToUniversalTime(), TimeSpan.Zero),
						};


						triggered.Add(new Tuple<Trigger, TriggerInvocation, DataPoint>(t, inv, datapoint));
					}
				}
			}

			var distinct = triggered.GroupBy(t => t.Item2.TriggerId)
				.Select(g => g.First()).ToList();

			await this.m_matcher.HandleTriggerAsync(distinct).AwaitBackground();
			await triggersdb.AddInvocationsAsync(distinct.Select(t => t.Item2)).AwaitBackground();

			this.logger.LogDebug($"{triggered.Count} triggers triggered!");
			this.logger.LogDebug($"{distinct.Count} triggers handled!");
		}
	}
}

