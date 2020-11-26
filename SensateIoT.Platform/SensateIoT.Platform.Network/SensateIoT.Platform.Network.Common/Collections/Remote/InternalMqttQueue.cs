/*
 * Remote MQTT queue implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Google.Protobuf;

using Microsoft.Extensions.Options;

using SensateIoT.Platform.Network.Common.Converters;
using SensateIoT.Platform.Network.Common.Helpers;
using SensateIoT.Platform.Network.Common.Infrastructure;
using SensateIoT.Platform.Network.Common.Settings;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.Common.Collections.Remote
{
	public class InternalMqttQueue : IRemoteQueue
	{
		private IDictionary<string, TextMessageData> m_textMessageQueues;
		private IDictionary<string, MeasurementData> m_measurementQueues;

		private SpinLockWrapper m_liveDataLock;
		private HashSet<string> m_liveDataHandlers;

		private SpinLockWrapper m_measurementLock;
		private SpinLockWrapper m_messageLock;

		private MeasurementData m_triggerMeasurements;
		private TextMessageData m_triggerMessages;

		private readonly string m_messageTriggerTopic;
		private readonly string m_measurementTriggerTopic;

		private readonly string m_messageLiveDataTopic;
		private readonly string m_measurementLiveDataTopic;

		private readonly IInternalMqttClient m_client;

		public InternalMqttQueue(IOptions<QueueSettings> options, IInternalMqttClient client)
		{
			this.m_liveDataLock = new SpinLockWrapper();
			this.m_liveDataHandlers = new HashSet<string>();
			this.m_textMessageQueues = new Dictionary<string, TextMessageData>();
			this.m_measurementQueues = new Dictionary<string, MeasurementData>();

			this.m_messageTriggerTopic = options.Value.TriggerQueueTemplate.Replace("$type", "messages");
			this.m_measurementTriggerTopic = options.Value.TriggerQueueTemplate.Replace("$type", "measurements");

			this.m_messageLiveDataTopic = options.Value.LiveDataQueueTemplate.Replace("$type", "messages");
			this.m_measurementLiveDataTopic = options.Value.LiveDataQueueTemplate.Replace("$type", "measurements");

			this.m_triggerMeasurements = new MeasurementData();
			this.m_triggerMessages = new TextMessageData();

			this.m_measurementLock = new SpinLockWrapper();
			this.m_messageLock = new SpinLockWrapper();
			this.m_client = client;
		}

		public void EnqueueToMessageTriggerService(IPlatformMessage message)
		{
			this.m_measurementLock.Lock();

			try {
				this.m_triggerMessages.Messages.Add(MessageProtobufConverter.Convert(message as Message));
			} finally {
				this.m_measurementLock.Unlock();
			}
		}

		public void EnqueueMeasurementToTriggerService(IPlatformMessage message)
		{
			this.m_measurementLock.Lock();

			try {
				this.m_triggerMeasurements.Measurements.Add(MeasurementProtobufConverter.Convert(message as Data.DTO.Measurement));
			} finally {
				this.m_measurementLock.Unlock();
			}
		}

		public void EnqueueMeasurementToTarget(IPlatformMessage message, RoutingTarget target)
		{
			this.m_liveDataLock.Lock();

			try {
				this.m_measurementQueues[target.Target].Measurements.Add(MeasurementProtobufConverter.Convert(message as Data.DTO.Measurement));
			} finally {
				this.m_liveDataLock.Unlock();
			}
		}

		public void EnqueueMessageToTarget(IPlatformMessage message, RoutingTarget target)
		{
			this.m_liveDataLock.Lock();

			try {
				this.m_textMessageQueues[target.Target].Messages.Add(MessageProtobufConverter.Convert(message as Message));
			} finally {
				this.m_liveDataLock.Unlock();
			}
		}

		public async Task FlushLiveDataAsync()
		{
			var publishes = new ConcurrentBag<Task>();
			IDictionary<string, TextMessageData> messages;
			IDictionary<string, MeasurementData> measurements;

			this.m_liveDataLock.Lock();
			this.m_measurementLock.Lock();
			this.m_messageLock.Lock();

			try {
				measurements = this.m_measurementQueues;
				messages = this.m_textMessageQueues;

				this.m_measurementQueues = new Dictionary<string, MeasurementData>();
				this.m_textMessageQueues = new Dictionary<string, TextMessageData>();
			} finally {
				this.m_messageLock.Unlock();
				this.m_measurementLock.Unlock();
				this.m_liveDataLock.Unlock();
			}


			Parallel.ForEach(measurements, async (kvp) => {
				if(kvp.Value.Measurements.Count <= 0) {
					return;
				}

				await using var stream = new MemoryStream();
				kvp.Value.WriteTo(stream);
				var data = stream.ToArray().Compress();
				var topic = this.m_measurementLiveDataTopic.Replace("$target", kvp.Key);

				publishes.Add(this.m_client.PublishOnAsync(topic, data, false));
			});

			Parallel.ForEach(messages, async (kvp) => {
				if(kvp.Value.Messages.Count <= 0) {
					return;
				}

				await using var stream = new MemoryStream();
				kvp.Value.WriteTo(stream);
				var data = stream.ToArray().Compress();
				var topic = this.m_messageLiveDataTopic.Replace("$target", kvp.Key);

				publishes.Add(this.m_client.PublishOnAsync(topic, data, false));
			});

			await Task.WhenAll(publishes).ConfigureAwait(false);
		}

		public async Task FlushAsync()
		{
			MeasurementData measurements;
			TextMessageData messages;

			this.m_measurementLock.Lock();
			this.m_messageLock.Lock();

			try {
				measurements = this.m_triggerMeasurements;
				messages = this.m_triggerMessages;

				this.m_triggerMeasurements = new MeasurementData();
				this.m_triggerMessages = new TextMessageData();
			} finally {
				this.m_messageLock.Unlock();
				this.m_measurementLock.Unlock();
			}

			await Task.WhenAll(
				this.PublishData(this.m_messageTriggerTopic, messages),
				this.PublishData(this.m_measurementTriggerTopic, measurements)
			).ConfigureAwait(false);
		}

		private Task PublishData(string topic, IMessage messages)
		{
			var size = messages.CalculateSize();

			if(size == 0) {
				return Task.CompletedTask;
			}

			using var stream = new MemoryStream();
			messages.WriteTo(stream);
			var data = stream.ToArray().Compress();

			return this.m_client.PublishOnAsync(topic, data, false);
		}

		public void SyncLiveDataHandlers(IEnumerable<LiveDataHandler> handlers)
		{
			this.m_liveDataLock.Lock();
			this.m_measurementLock.Lock();
			this.m_messageLock.Lock();

			try {
				var names = handlers.Select(x => x.Name).ToList();
				var dropped = this.m_liveDataHandlers.Except(names);

				this.m_liveDataHandlers = new HashSet<string>();
				this.m_liveDataHandlers.UnionWith(names);

				foreach(var name in dropped) {
					this.m_measurementQueues.Remove(name);
					this.m_textMessageQueues.Remove(name);
				}

				foreach(var handler in this.m_liveDataHandlers) {
					this.m_measurementQueues.TryAdd(handler, new MeasurementData());
					this.m_textMessageQueues.TryAdd(handler, new TextMessageData());
				}
			} finally {
				this.m_messageLock.Unlock();
				this.m_measurementLock.Unlock();
				this.m_liveDataLock.Unlock();
			}
		}
	}
}
