/*
 * MQTT measurement handler.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateIoT.Platform.Network.Common.Converters;
using SensateIoT.Platform.Network.Common.MQTT;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.DataAccess.Repositories;
using SensateIoT.Platform.Network.StorageService.DTO;

namespace SensateIoT.Platform.Network.StorageService.MQTT
{
	public class MqttBulkMeasurementHandler : IMqttHandler
	{
		private readonly ILogger<MqttBulkMeasurementHandler> logger;
		private readonly IMeasurementRepository m_measurements;
		private readonly ISensorStatisticsRepository m_stats;

		public MqttBulkMeasurementHandler(IMeasurementRepository measurements, ISensorStatisticsRepository stats, ILogger<MqttBulkMeasurementHandler> logger)
		{
			this.logger = logger;
			this.m_measurements = measurements;
			this.m_stats = stats;
		}

		public async Task OnMessageAsync(string topic, string message, CancellationToken ct)
		{
			try {
				var measurementMap = MeasurementDatabaseConverter.Convert(DeserializeMeasurements(message));
				var stats = measurementMap.Select(m => new StatisticsUpdate(RequestMethod.Any, m.Value.Count, m.Key));

				await Task.WhenAll(
					this.m_measurements.StoreAsync(measurementMap, ct),
					this.IncrementStatistics(stats.ToList(), ct)
				).ConfigureAwait(false);
			} catch(Exception ex) {
				this.logger.LogInformation($"Error: {ex.Message}");
				this.logger.LogInformation($"Received a buggy MQTT message: {message}");
			}
		}

		private static MeasurementData DeserializeMeasurements(string data)
		{
			var bytes = Convert.FromBase64String(data);
			using var to = new MemoryStream();
			using var from = new MemoryStream(bytes);
			using var gzip = new GZipStream(@from, CompressionMode.Decompress);

			gzip.CopyTo(to);
			var final = to.ToArray();
			return MeasurementData.Parser.ParseFrom(final);
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
