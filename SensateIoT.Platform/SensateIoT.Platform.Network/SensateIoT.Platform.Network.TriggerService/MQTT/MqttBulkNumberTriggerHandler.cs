/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;

using SensateIoT.Platform.Network.Common.MQTT;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.TriggerService.DTO;
using SensateIoT.Platform.Network.TriggerService.Services;

using Convert = System.Convert;
using DataPoint = SensateIoT.Platform.Network.Data.DTO.DataPoint;

namespace SensateIoT.Platform.Network.TriggerService.MQTT
{
	public class MqttBulkNumberTriggerHandler : IMqttHandler
	{
		private readonly IServiceProvider m_provider;
		private readonly ILogger<MqttBulkNumberTriggerHandler> logger;
		private readonly IDataPointMatchingService m_matcher;

		public MqttBulkNumberTriggerHandler(IServiceProvider provider,
											IDataPointMatchingService matcher,
											ILogger<MqttBulkNumberTriggerHandler> logger)
		{
			this.m_provider = provider;
			this.logger = logger;
			this.m_matcher = matcher;
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
				group measurement by measurement.SensorID into g
				select new InternalBulkMeasurements {
					SensorID = ObjectId.Parse(g.Key),
					Measurements = g.Select(m => new SingleMeasurement {
						Data = m.Datapoints.ToDictionary(p => p.Key, p => new DataPoint {
							Accuracy = p.Accuracy,
							Precision = p.Precision,
							Unit = p.Unit,
							Value = Convert.ToDecimal(p.Value),
						}),
						Location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(m.Longitude, m.Latitude)),
						PlatformTime = m.PlatformTime.ToDateTime(),
						Timestamp = m.Timestamp.ToDateTime()
					}).ToList()
				};

			return measurements;
		}

		public async Task OnMessageAsync(string topic, string message, CancellationToken ct)
		{
			this.logger.LogDebug("Measurement received.");
			var tasks = new List<Task>();

			var measurements = Decompress(message).ToList();
			using var scope = this.m_provider.CreateScope();
			var triggersdb = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();
			var triggers = await triggersdb.GetTriggerServiceActions(measurements.Select(x => x.SensorID), ct).ConfigureAwait(false);
			var triggerMap = triggers
				.GroupBy(x => x.SensorID, x => x)
				.ToDictionary(x => x.Key, x => x.ToList());
			var exec = scope.ServiceProvider.GetRequiredService<ITriggerActionExecutionService>();

			foreach(var metaMeasurement in measurements) {
				var actions = triggerMap[metaMeasurement.SensorID];

				if(actions == null) {
					continue;
				}

				foreach(var m in metaMeasurement.Measurements) {
					foreach(var keyValuePair in m.Data) {
						var matched = this.m_matcher.Match(keyValuePair.Key, keyValuePair.Value, actions);
						tasks.Add(ExecuteActionsAsync(exec, matched, keyValuePair.Value));
					}
				}
			}

			await Task.WhenAll(tasks);

			this.logger.LogDebug("Measurement handled.");
		}

		private static Task ExecuteActionsAsync(ITriggerActionExecutionService exec, IEnumerable<TriggerAction> actions, DataPoint dp)
		{
			var tasks = new List<Task>();

			foreach(var action in actions) {
				var result = Replace(action.Message, dp);
				tasks.Add(exec.ExecuteAsync(action, result));
			}

			return Task.WhenAll(tasks);
		}

		private static string Replace(string message, DataPoint dp)
		{
			string precision;
			string accuracy;
			var body = message.Replace("$value", dp.Value.ToString(CultureInfo.InvariantCulture));

			body = body.Replace("$unit", dp.Unit);

			precision = dp.Precision != null ? dp.Precision.Value.ToString(CultureInfo.InvariantCulture) : "";
			accuracy = dp.Accuracy != null ? dp.Accuracy.Value.ToString(CultureInfo.InvariantCulture) : "";

			body = body.Replace("$precision", precision);
			body = body.Replace("$accuracy", accuracy);

			return body;
		}
	}
}

