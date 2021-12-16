using System.Collections.Generic;

using Microsoft.Extensions.Options;

using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.MQTT;
using SensateIoT.Platform.Router.Common.Services.Abstract;
using SensateIoT.Platform.Router.Common.Settings;
using SensateIoT.Platform.Router.Data.Abstract;

namespace SensateIoT.Platform.Router.Common.Services.Metrics
{
	public class HealthMonitoringService : IHealthMonitoringService
	{
		private readonly IInternalRemoteQueue m_internalRemoteQueues;
		private readonly IPublicRemoteQueue m_publicQueue;
		private readonly IQueue<IPlatformMessage> m_inputQueue;
		private readonly HealthCheckSettings m_settings;
		private readonly IPublicMqttClient m_publicMqttClient;
		private readonly IInternalMqttClient m_internalMqttClient;

		public bool IsHealthy => this.GetHealthStatus();

		public HealthMonitoringService(IQueue<IPlatformMessage> inputQueue,
		                               IInternalRemoteQueue @internal,
		                               IPublicRemoteQueue @public,
									   IPublicMqttClient publicClient,
									   IInternalMqttClient internalClient,
		                               IOptions<HealthCheckSettings> settings)
		{
			this.m_internalRemoteQueues = @internal;
			this.m_publicQueue = @public;
			this.m_inputQueue = inputQueue;
			this.m_internalMqttClient = internalClient;
			this.m_publicMqttClient = publicClient;
			this.m_settings = settings.Value;
		}

		public IEnumerable<string> GetHealthStatusExplanation()
		{
			var result = new List<string>();

			this.UpdateQueueReasons(result);
			this.UpdateMqttReasons(result);

			return result;
		}

		private void UpdateMqttReasons(ICollection<string> reasons)
		{
			if(!this.m_internalMqttClient.IsConnected) {
				reasons.Add("Internal MQTT client is disconnected");
			}

			if(!this.m_publicMqttClient.IsConnected) {
				reasons.Add("Public MQTT client is disconnected");
			}
		}

		private void UpdateQueueReasons(ICollection<string> result)
		{
			if(!this.CheckLiveDataQueues()) {
				result.Add("Live Data Service queue out of bounds");
			}

			if(!this.CheckTriggerQueues()) {
				result.Add("Trigger Service queue out of bounds");
			}

			if(!this.CheckPublicMqttQueue()) {
				result.Add("Public MQTT queue is out of bounds");
			}

			if(!this.CheckInputQueue()) {
				result.Add("Router input queue is out of bounds");
			}
		}

		private bool GetHealthStatus()
		{
			return this.CheckLiveDataQueues() && this.CheckTriggerQueues() && this.CheckPublicMqttQueue() &&
			       this.CheckInputQueue() && this.m_internalMqttClient.IsConnected && this.m_publicMqttClient.IsConnected;
		}

		private bool CheckInputQueue()
		{
			var limit = this.m_settings.InputQueueLimit ?? this.m_settings.DefaultQueueLimit;
			return this.m_inputQueue.Count <= limit;
		}

		private bool CheckPublicMqttQueue()
		{
			var limit = this.m_settings.PublicMqttQueueLimit ?? this.m_settings.DefaultQueueLimit;
			return this.m_publicQueue.QueueLength <= limit;
		}

		private bool CheckLiveDataQueues()
		{
			var limit = this.m_settings.LiveDataServiceQueueLimit ?? this.m_settings.DefaultQueueLimit;
			return this.m_internalRemoteQueues.LiveDataQueueLength <= limit;
		}

		private bool CheckTriggerQueues()
		{
			var limit = this.m_settings.TriggerServiceQueueLimit ?? this.m_settings.DefaultQueueLimit;
			return this.m_internalRemoteQueues.TriggerQueueLength <= limit;
		}
	}
}
