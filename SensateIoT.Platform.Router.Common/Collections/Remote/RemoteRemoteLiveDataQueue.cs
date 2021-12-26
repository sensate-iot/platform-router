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

using Microsoft.Extensions.Options;

using Google.Protobuf;
using Prometheus;

using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Converters;
using SensateIoT.Platform.Router.Common.Helpers;
using SensateIoT.Platform.Router.Common.MQTT;
using SensateIoT.Platform.Router.Common.Settings;
using SensateIoT.Platform.Router.Contracts.DTO;
using SensateIoT.Platform.Router.Data.Abstract;
using SensateIoT.Platform.Router.Data.DTO;
using SensateIoT.Platform.Router.Data.Models;

using ControlMessage = SensateIoT.Platform.Router.Data.DTO.ControlMessage;
using Measurement = SensateIoT.Platform.Router.Data.DTO.Measurement;
using Message = SensateIoT.Platform.Router.Data.DTO.Message;

namespace SensateIoT.Platform.Router.Common.Collections.Remote
{
	public class RemoteRemoteLiveDataQueue : IRemoteLiveDataQueue
	{
		private IDictionary<string, TextMessageData> m_textMessageQueues;
		private IDictionary<string, MeasurementData> m_measurementQueues;
		private IDictionary<string, ControlMessageData> m_controlMessageQueues;

		private SpinLockWrapper m_liveDataLock;
		private HashSet<string> m_liveDataHandlers;

		private SpinLockWrapper m_measurementLock;
		private SpinLockWrapper m_messageLock;
		private SpinLockWrapper m_controlLock;

		private readonly string m_messageLiveDataTopic;
		private readonly string m_measurementLiveDataTopic;
		private readonly string m_controlMessageLiveDataTopic;

		private readonly Gauge m_gaugeLiveDataService;

		private readonly IInternalMqttClient m_client;

		public RemoteRemoteLiveDataQueue(IOptions<QueueSettings> options, IInternalMqttClient client)
		{
			this.m_liveDataLock = new SpinLockWrapper();
			this.m_liveDataHandlers = new HashSet<string>();
			this.m_textMessageQueues = new Dictionary<string, TextMessageData>();
			this.m_measurementQueues = new Dictionary<string, MeasurementData>();
			this.m_controlMessageQueues = new Dictionary<string, ControlMessageData>();

			this.m_controlMessageLiveDataTopic = options.Value.LiveDataQueueTemplate.Replace("$type", "control");
			this.m_messageLiveDataTopic = options.Value.LiveDataQueueTemplate.Replace("$type", "messages");
			this.m_measurementLiveDataTopic = options.Value.LiveDataQueueTemplate.Replace("$type", "measurements");

			this.m_controlLock = new SpinLockWrapper();
			this.m_measurementLock = new SpinLockWrapper();
			this.m_messageLock = new SpinLockWrapper();
			this.m_client = client;
			this.m_gaugeLiveDataService = Metrics.CreateGauge("router_livedata_messages_queued", "Number of messages in the live data queue.");
		}

		public int Count => this.GetLiveDataQueueSize();

		public void EnqueueMeasurementToTarget(IPlatformMessage message, RoutingTarget target)
		{
			this.m_liveDataLock.Lock();

			try {
				this.m_measurementQueues[target.Target].Measurements.Add(MeasurementProtobufConverter.Convert(message as Measurement));
				this.m_gaugeLiveDataService.Inc();
			} finally {
				this.m_liveDataLock.Unlock();
			}
		}

		public void EnqueueMessageToTarget(IPlatformMessage message, RoutingTarget target)
		{
			this.m_liveDataLock.Lock();

			try {
				this.m_textMessageQueues[target.Target].Messages.Add(MessageProtobufConverter.Convert(message as Message));
				this.m_gaugeLiveDataService.Inc();
			} finally {
				this.m_liveDataLock.Unlock();
			}
		}

		public void EnqueueControlMessageToTarget(IPlatformMessage message, RoutingTarget target)
		{
			this.m_liveDataLock.Lock();

			try {
				this.m_controlMessageQueues[target.Target].Messages.Add(ControlMessageProtobufConverter.Convert(message as ControlMessage));
				this.m_gaugeLiveDataService.Inc();
			} finally {
				this.m_liveDataLock.Unlock();
			}
		}

		public async Task FlushAsync()
		{
			IDictionary<string, TextMessageData> messages;
			IDictionary<string, MeasurementData> measurements;
			IDictionary<string, ControlMessageData> controlMessages;

			this.AcquireLiveDataLocks();

			try {
				measurements = this.m_measurementQueues;
				messages = this.m_textMessageQueues;
				controlMessages = this.m_controlMessageQueues;

				this.m_gaugeLiveDataService.Set(0D);
				this.m_measurementQueues = new Dictionary<string, MeasurementData>();
				this.m_textMessageQueues = new Dictionary<string, TextMessageData>();
				this.m_controlMessageQueues = new Dictionary<string, ControlMessageData>();

				foreach(var handler in this.m_liveDataHandlers) {
					this.m_measurementQueues.Add(handler, new MeasurementData());
					this.m_textMessageQueues.Add(handler, new TextMessageData());
					this.m_controlMessageQueues.Add(handler, new ControlMessageData());
				}
			} finally {
				this.ReleaseLiveDataLocks();
			}

			await this.PublishLiveData(controlMessages, messages, measurements).ConfigureAwait(false);
		}

		private async Task PublishLiveData(IDictionary<string, ControlMessageData> control,
										   IDictionary<string, TextMessageData> messages,
										   IDictionary<string, MeasurementData> measurements)
		{
			var publishes = new ConcurrentBag<Task>();

			this.ProcessMeasurements(measurements, publishes);
			this.ProcessMessages(messages, publishes);
			this.ProcessControlMessages(control, publishes);

			await Task.WhenAll(publishes).ConfigureAwait(false);
		}

		private void ProcessMeasurements(IDictionary<string, MeasurementData> measurements, ConcurrentBag<Task> publishes)
		{
			Parallel.ForEach(measurements, async kvp => {
				if(kvp.Value.Measurements.Count <= 0) {
					return;
				}

				await using var stream = new MemoryStream();
				kvp.Value.WriteTo(stream);
				var data = stream.ToArray().Compress();
				var topic = this.m_measurementLiveDataTopic.Replace("$target", kvp.Key);

				publishes.Add(this.m_client.PublishOnAsync(topic, data, false));
			});
		}

		private void ProcessMessages(IDictionary<string, TextMessageData> messages, ConcurrentBag<Task> publishes)
		{
			Parallel.ForEach(messages, async kvp => {
				if(kvp.Value.Messages.Count <= 0) {
					return;
				}

				await using var stream = new MemoryStream();
				kvp.Value.WriteTo(stream);
				var data = stream.ToArray().Compress();
				var topic = this.m_messageLiveDataTopic.Replace("$target", kvp.Key);

				publishes.Add(this.m_client.PublishOnAsync(topic, data, false));
			});
		}

		private void ProcessControlMessages(IDictionary<string, ControlMessageData> control, ConcurrentBag<Task> publishes)
		{
			Parallel.ForEach(control, async kvp => {
				if(kvp.Value.Messages.Count <= 0) {
					return;
				}

				await using var stream = new MemoryStream();
				kvp.Value.WriteTo(stream);
				var data = stream.ToArray().Compress();
				var topic = this.m_controlMessageLiveDataTopic.Replace("$target", kvp.Key);

				publishes.Add(this.m_client.PublishOnAsync(topic, data, false));
			});
		}

		private int GetLiveDataQueueSize()
		{
			int size;
			this.AcquireLiveDataLocks();

			try {
				size = this.UnzipQueueSize();
			} finally {
				this.ReleaseLiveDataLocks();
			}

			return size;
		}

		private void AcquireLiveDataLocks()
		{
			this.m_liveDataLock.Lock();
			this.m_measurementLock.Lock();
			this.m_messageLock.Lock();
			this.m_controlLock.Lock();
		}

		private void ReleaseLiveDataLocks()
		{
			this.m_controlLock.Unlock();
			this.m_messageLock.Unlock();
			this.m_measurementLock.Unlock();
			this.m_liveDataLock.Unlock();
		}

		private int UnzipQueueSize()
		{
			var total = 0;

			foreach(var (_, value) in this.m_measurementQueues) {
				total += value.Measurements.Count;
			}

			foreach(var (_, value) in this.m_textMessageQueues) {
				total += value.Messages.Count;
			}

			foreach(var (_, value) in this.m_controlMessageQueues) {
				total += value.Messages.Count;
			}

			return total;
		}

		public void SyncLiveDataHandlers(IEnumerable<LiveDataHandler> handlers)
		{
			this.AcquireLiveDataLocks();

			try {
				var names = handlers.Select(x => x.Name).ToList();
				var dropped = this.m_liveDataHandlers.Except(names);

				this.m_liveDataHandlers = new HashSet<string>();
				this.m_liveDataHandlers.UnionWith(names);

				foreach(var name in dropped) {
					this.m_measurementQueues.Remove(name);
					this.m_textMessageQueues.Remove(name);
					this.m_controlMessageQueues.Remove(name);
				}

				foreach(var handler in this.m_liveDataHandlers) {
					this.m_measurementQueues.TryAdd(handler, new MeasurementData());
					this.m_textMessageQueues.TryAdd(handler, new TextMessageData());
					this.m_controlMessageQueues.TryAdd(handler, new ControlMessageData());
				}
			} finally {
				this.ReleaseLiveDataLocks();
			}
		}
	}
}
