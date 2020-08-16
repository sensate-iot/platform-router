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
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MongoDB.Driver.GeoJsonObjectModel;

using SensateService.Common.Data.Models;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Protobuf;
using SensateService.TriggerHandler.Models;
using SensateService.TriggerHandler.Services;
using SensateService.TriggerHandler.Utils;

using Convert = System.Convert;
using DataPoint = SensateService.Common.Data.Models.DataPoint;
using Measurement = SensateService.Common.Data.Models.Measurement;

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

		private static IEnumerable<InternalBulkMeasurements> Decompress(string data)
		{
			var bytes = Convert.FromBase64String(data);
			using var to = new MemoryStream();
			using var from = new MemoryStream(bytes);
			using var gzip = new GZipStream(@from, CompressionMode.Decompress);

			gzip.CopyTo(to);
			var final = to.ToArray();
			var protoMeasurements = MeasurementData.Parser.ParseFrom(final);
			var measurements =
				from measurement in protoMeasurements.Measurements
				group measurement by measurement.SensorId into g
				select new InternalBulkMeasurements {
					CreatedBy = g.Key,
					Measurements = g.Select(m => new Measurement {
						Data = m.Datapoints.ToDictionary(p => p.Key, p => new DataPoint {
							Accuracy = p.Accuracy,
							Precision = p.Precision,
							Unit = p.Unit,
							Value = Convert.ToDecimal(p.Value),
						}),
						Location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(m.Longitude, m.Latitude)),
						PlatformTime = DateTime.Parse(m.PlatformTime),
						Timestamp = DateTime.Parse(m.Timestamp)
					}).ToList()
				};

			return measurements;
		}

		public override async Task OnMessageAsync(string topic, string message)
		{
			this.logger.LogDebug("Message received!");

			var measurements = Decompress(message).ToList();
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

