/*
 * MQTT handler for incoming messages.
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
using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.StorageService.DTO;

namespace SensateIoT.Platform.Network.StorageService.MQTT
{
	[UsedImplicitly]
	public class MqttBulkMessageHandler : IMqttHandler
	{
		private readonly ILogger<MqttBulkMessageHandler> m_logger;
		private readonly IMessageRepository m_messages;
		private readonly ISensorStatisticsRepository m_stats;
		private readonly Counter m_storageCounter;
		private readonly Histogram m_duration;

		public MqttBulkMessageHandler(IMessageRepository message,
									  ISensorStatisticsRepository stats,
		                              ILogger<MqttBulkMessageHandler> logger)
		{
			this.m_logger = logger;
			this.m_stats = stats;
			this.m_messages = message;
			this.m_storageCounter = Metrics.CreateCounter("storageservice_messages_stored_total", "Total number of messages stored.");
			this.m_duration = Metrics.CreateHistogram("storageservice_message_storage_duration_seconds", "Histogram of message storage duration.");
		}

		public async Task OnMessageAsync(string topic, string message, CancellationToken ct)
		{
			var sw = Stopwatch.StartNew();

			try {
				using(this.m_duration.NewTimer()) {
					var databaseMessages = this.Decompress(message).ToList();
					var stats = databaseMessages
						.Select(m => new StatisticsUpdate(StatisticsType.MessageStorage, 1, m.SensorId)).ToList();

					this.m_storageCounter.Inc(databaseMessages.Count);
					this.m_logger.LogInformation("Attempting to store {count} messages.", databaseMessages.Count);
					await Task.WhenAll(this.m_messages.CreateRangeAsync(databaseMessages, ct),
					                   this.IncrementStatistics(stats, ct)).ConfigureAwait(false);

				}

			} catch(Exception ex) {
				this.m_logger.LogWarning("Unable to store message: {exception} " +
										 "Message content: {message}. " +
										 "Stack trace: ", ex.Message, message, ex.StackTrace);
			}

			sw.Stop();
			this.m_logger.LogInformation("Storage attempt of messages took {timespan}.", sw.Elapsed.ToString("c"));
		}

		private IEnumerable<Message> Decompress(string data)
		{
			var bytes = Convert.FromBase64String(data);
			using var to = new MemoryStream();
			using var from = new MemoryStream(bytes);
			using var gzip = new GZipStream(from, CompressionMode.Decompress);

			gzip.CopyTo(to);
			var final = to.ToArray();
			var protoMeasurements = TextMessageData.Parser.ParseFrom(final);
			this.m_logger.LogInformation("Storing {count} messages!", protoMeasurements.Messages.Count);
			return MessageDatabaseConverter.Convert(protoMeasurements);
		}

		private async Task IncrementStatistics(ICollection<StatisticsUpdate> data, CancellationToken token)
		{
			var tasks = new Task[data.Count];

			for(var idx = 0; idx < data.Count; idx++) {
				var entry = data.ElementAt(idx);
				tasks[idx] = this.m_stats.IncrementManyAsync(entry.SensorId, entry.Type, entry.Count, token);
			}

			await Task.WhenAll(tasks).ConfigureAwait(false);
		}
	}
}
