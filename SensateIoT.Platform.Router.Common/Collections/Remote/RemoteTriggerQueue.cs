using System.IO;
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

using Measurement = SensateIoT.Platform.Router.Data.DTO.Measurement;

namespace SensateIoT.Platform.Router.Common.Collections.Remote
{
	public class RemoteTriggerQueue : IRemoteTriggerQueue
	{
		private SpinLockWrapper m_measurementLock;
		private SpinLockWrapper m_messageLock;

		private MeasurementData m_triggerMeasurements;
		private TextMessageData m_triggerMessages;
		private readonly string m_messageTriggerTopic;
		private readonly string m_measurementTriggerTopic;
		private readonly IInternalMqttClient m_client;
		private readonly Gauge m_gaugeTriggerSerivce;

		public int Count => this.GetTriggerQueueLength();

		public RemoteTriggerQueue(IOptions<QueueSettings> options, IInternalMqttClient client)
		{
			this.m_gaugeTriggerSerivce = Metrics.CreateGauge("router_trigger_messages_queued", "Number of messages in the trigger queue.");

			this.m_triggerMeasurements = new MeasurementData();
			this.m_triggerMessages = new TextMessageData();
			this.m_measurementLock = new SpinLockWrapper();
			this.m_messageLock = new SpinLockWrapper();

			this.m_client = client;
			this.m_messageTriggerTopic = options.Value.TriggerQueueTemplate.Replace("$type", "messages");
			this.m_measurementTriggerTopic = options.Value.TriggerQueueTemplate.Replace("$type", "measurements");
		}

		public void EnqueueMessageToTriggerService(IPlatformMessage message)
		{
			this.m_measurementLock.Lock();

			try {
				this.m_triggerMessages.Messages.Add(MessageProtobufConverter.Convert(message as Message));
				this.m_gaugeTriggerSerivce.Inc();
			} finally {
				this.m_measurementLock.Unlock();
			}
		}

		public void EnqueueMeasurementToTriggerService(IPlatformMessage message)
		{
			this.m_measurementLock.Lock();

			try {
				this.m_triggerMeasurements.Measurements.Add(MeasurementProtobufConverter.Convert(message as Measurement));
				this.m_gaugeTriggerSerivce.Inc();
			} finally {
				this.m_measurementLock.Unlock();
			}
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

				this.m_gaugeTriggerSerivce.Set(0D);
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


		private int GetTriggerQueueLength()
		{
			var size = 0;
			this.m_measurementLock.Lock();
			this.m_messageLock.Lock();

			try {
				size += this.m_triggerMeasurements.Measurements.Count;
				size += this.m_triggerMessages.Messages.Count;
			} finally {
				this.m_messageLock.Unlock();
				this.m_measurementLock.Unlock();
			}

			return size;
		}
	}
}