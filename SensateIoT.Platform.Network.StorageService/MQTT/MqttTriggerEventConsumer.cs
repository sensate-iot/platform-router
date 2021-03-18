using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBrains.Annotations;
using MongoDB.Bson;

using SensateIoT.Platform.Network.Common.MQTT;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.Enums;
using SensateIoT.Platform.Network.DataAccess.Abstract;

namespace SensateIoT.Platform.Network.StorageService.MQTT
{
	[UsedImplicitly]
	public class MqttTriggerEventConsumer : IMqttHandler
	{
		private readonly ISensorStatisticsRepository m_stats;
		private readonly ILogger<MqttTriggerEventConsumer> m_logger;

		public MqttTriggerEventConsumer(ISensorStatisticsRepository stats, ILogger<MqttTriggerEventConsumer> logger)
		{
			this.m_stats = stats;
			this.m_logger = logger;
		}

		public async Task OnMessageAsync(string topic, string message, CancellationToken ct = default)
		{
			var data = this.Decompress(message);
			var tasks = new List<Task>();

			foreach(var triggerEvent in data) {
				var id = new ObjectId(triggerEvent.SensorID.ToByteArray());
				tasks.Add(this.m_stats.IncrementManyAsync(id, ConvertTriggerType(triggerEvent.Type), 1, ct));
			}

			await Task.WhenAll(tasks).ConfigureAwait(false);
		}

		private IEnumerable<TriggerEvent> Decompress(string data)
		{
			var bytes = Convert.FromBase64String(data);
			using var to = new MemoryStream();
			using var from = new MemoryStream(bytes);
			using var gzip = new GZipStream(from, CompressionMode.Decompress);

			gzip.CopyTo(to);
			var final = to.ToArray();
			var triggerEventData = TriggerEventData.Parser.ParseFrom(final);
			this.m_logger.LogInformation("Storing {count} messages!", triggerEventData.Events.Count);

			return triggerEventData.Events;
		}

		private static StatisticsType ConvertTriggerType(TriggerEventType type)
		{
			var result = type switch {
				TriggerEventType.Email => StatisticsType.Email,
				TriggerEventType.Sms => StatisticsType.SMS,
				TriggerEventType.LiveData => StatisticsType.LiveData,
				TriggerEventType.Mqtt => StatisticsType.MQTT,
				TriggerEventType.HttpPost => StatisticsType.HttpPost,
				TriggerEventType.HttpGet => StatisticsType.HttpGet,
				TriggerEventType.ControlMessage => StatisticsType.ControlMessage,
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};

			return result;
		}
	}
}
