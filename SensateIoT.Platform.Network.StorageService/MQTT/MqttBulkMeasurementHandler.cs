/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBrains.Annotations;
using Prometheus;

using SensateIoT.Platform.Network.Common.Converters;
using SensateIoT.Platform.Network.Common.MQTT;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.Enums;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.StorageService.DTO;

namespace SensateIoT.Platform.Network.StorageService.MQTT
{
	[UsedImplicitly]
	public class MqttBulkMeasurementHandler : IMqttHandler
	{
		private readonly ILogger<MqttBulkMeasurementHandler> m_logger;
		private readonly IMeasurementRepository m_measurements;
		private readonly ISensorStatisticsRepository m_stats;
		private readonly Counter m_storageCounter;
		private readonly Histogram m_duration;

		public MqttBulkMeasurementHandler(IMeasurementRepository measurements, ISensorStatisticsRepository stats, ILogger<MqttBulkMeasurementHandler> logger)
		{
			this.m_logger = logger;
			this.m_measurements = measurements;
			this.m_stats = stats;
			this.m_storageCounter = Metrics.CreateCounter("storageservice_messages_stored_total", "Total number of messages stored.");
			this.m_duration = Metrics.CreateHistogram("storageservice_measurement_storage_duration_seconds", "Histogram of measurement storage duration.");
		}

		public async Task OnMessageAsync(string topic, string message, CancellationToken ct)
		{
			var sw = Stopwatch.StartNew();

			try {

				using(this.m_duration.NewTimer()) {
					await this.HandleMessage(message, ct).ConfigureAwait(false);
				}

			} catch(Exception ex) {
				this.m_logger.LogInformation($"Error: {ex.Message}");
				this.m_logger.LogInformation($"Received a buggy MQTT message: {message}");
			}
			sw.Stop();
			this.m_logger.LogInformation("Storage attempt of measurements took {timespan}.", sw.Elapsed.ToString("c"));
		}

		private async Task HandleMessage(string message, CancellationToken ct)
		{
			var measurementMap = MeasurementDatabaseConverter.Convert(this.DeserializeMeasurements(message));
			var stats = measurementMap.Select(m => new StatisticsUpdate(RequestMethod.Any, m.Value.Count, m.Key));
			var count = measurementMap.Aggregate(0L, (l, pair) => l + pair.Value.Count);

			this.m_storageCounter.Inc(count);

			await Task.WhenAll(
				this.m_measurements.StoreAsync(measurementMap, ct),
				this.IncrementStatistics(stats.ToList(), ct)
			).ConfigureAwait(false);
		}

		private MeasurementData DeserializeMeasurements(string data)
		{
			var bytes = Convert.FromBase64String(data);
			using var to = new MemoryStream();
			using var from = new MemoryStream(bytes);
			using var gzip = new GZipStream(@from, CompressionMode.Decompress);

			gzip.CopyTo(to);
			var final = to.ToArray();

			var measurements = MeasurementData.Parser.ParseFrom(final);
			this.m_logger.LogInformation("Storing {count} measurements.", measurements.Measurements.Count);
			return measurements;
		}

		private async Task IncrementStatistics(ICollection<StatisticsUpdate> data, CancellationToken token)
		{
			var tasks = new Task[data.Count];

			for(var idx = 0; idx < data.Count; idx++) {
				var entry = data.ElementAt(idx);
				tasks[idx] = this.m_stats.IncrementManyAsync(entry.SensorId, entry.Method, entry.Count, token);
			}

			await Task.WhenAll(tasks).ConfigureAwait(false);
		}
	}
}
