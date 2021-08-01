/*
 * Remote storage queue implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Google.Protobuf;
using Prometheus;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Collections.Local;
using SensateIoT.Platform.Router.Common.Converters;
using SensateIoT.Platform.Router.Common.Helpers;
using SensateIoT.Platform.Router.Common.MQTT;
using SensateIoT.Platform.Router.Common.Settings;
using Measurement = SensateIoT.Platform.Network.Data.DTO.Measurement;

namespace SensateIoT.Platform.Router.Common.Collections.Remote
{
	public class RemoteStorageQueue : IRemoteStorageQueue
	{
		private readonly string m_measurementStorageQueue;
		private readonly string m_messageStorageQueue;

		private readonly IInternalMqttClient m_client;
		private readonly IQueue<Message> m_messages;
		private readonly IQueue<Measurement> m_measurements;
		private readonly Gauge m_gauge;

		public RemoteStorageQueue(IOptions<QueueSettings> options, IInternalMqttClient client)
		{
			this.m_measurementStorageQueue = options.Value.MeasurementStorageQueueTopic;
			this.m_messageStorageQueue = options.Value.MessageStorageQueueTopic;

			this.m_client = client;
			this.m_messages = new Deque<Message>();
			this.m_measurements = new Deque<Measurement>();
			this.m_gauge = Metrics.CreateGauge("router_storage_messages_queued", "Number of messages in the storage queue.");
		}

		public void Enqueue(IPlatformMessage message)
		{
			if(message.Type == MessageType.Measurement) {
				this.m_measurements.Add(message as Measurement);
			} else {
				this.m_messages.Add(message as Message);
			}

			this.m_gauge.Inc();
		}

		public async Task FlushMessagesAsync()
		{
			var publishes = new ConcurrentBag<Task>();

			this.m_gauge.Set(0D);
			var measurements = this.m_measurements.DequeueRange(int.MaxValue).ToList();
			var messages = this.m_messages.DequeueRange(int.MaxValue).ToList();

			if(measurements.Count > 0) {
				var protoMeasurements = MeasurementProtobufConverter.Convert(measurements);
				await using var measurementStream = new MemoryStream();
				protoMeasurements.WriteTo(measurementStream);
				var data = measurementStream.ToArray().Compress();
				publishes.Add(this.m_client.PublishOnAsync(this.m_measurementStorageQueue, data, false));
			}


			if(messages.Count > 0) {
				var protoMessages = MessageProtobufConverter.Convert(messages);
				await using var messageStream = new MemoryStream();
				protoMessages.WriteTo(messageStream);
				var messageData = messageStream.ToArray().Compress();
				publishes.Add(this.m_client.PublishOnAsync(this.m_messageStorageQueue, messageData, false));
			}


			await Task.WhenAll(publishes).ConfigureAwait(false);
		}
	}
}
