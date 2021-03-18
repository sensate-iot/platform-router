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
	public class MqttNetworkEventConsumer : IMqttHandler
	{
		private readonly ISensorStatisticsRepository m_stats;
		private readonly ILogger<MqttTriggerEventConsumer> m_logger;

		public MqttNetworkEventConsumer(ISensorStatisticsRepository stats, ILogger<MqttTriggerEventConsumer> logger)
		{
			this.m_stats = stats;
			this.m_logger = logger;
		}

		public async Task OnMessageAsync(string topic, string message, CancellationToken ct = default)
		{
			var data = this.Decompress(message);
			var tasks = new List<Task>();

			foreach(var networkEvent in data) {
				var id = new ObjectId(networkEvent.SensorID.ToByteArray());
				var count = 0;

				foreach(var action in networkEvent.Actions) {
					if(action == NetworkEventType.MessageLiveData ||
					   action == NetworkEventType.MessageTriggered) {
						count += 1;
					}
				}

				if(count > 0) {
					tasks.Add(this.m_stats.IncrementManyAsync(id, StatisticsType.MessageRouted, count, ct));
				}

			}

			await Task.WhenAll(tasks).ConfigureAwait(false);
		}

		private IEnumerable<NetworkEvent> Decompress(string data)
		{
			var bytes = Convert.FromBase64String(data);
			using var to = new MemoryStream();
			using var from = new MemoryStream(bytes);
			using var gzip = new GZipStream(from, CompressionMode.Decompress);

			gzip.CopyTo(to);
			var final = to.ToArray();
			var networkEventData = NetworkEventData.Parser.ParseFrom(final);
			this.m_logger.LogInformation("Storing {count} network events!", networkEventData.Events.Count);

			return networkEventData.Events;
		}
	}
}
