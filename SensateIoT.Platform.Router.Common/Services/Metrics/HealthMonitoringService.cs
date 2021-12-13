using System.Collections.Generic;

using Microsoft.Extensions.Options;

using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Services.Abstract;
using SensateIoT.Platform.Router.Common.Settings;

namespace SensateIoT.Platform.Router.Common.Services.Metrics
{
	public class HealthMonitoringService : IHealthMonitoringService
	{
		private readonly IInternalRemoteQueue m_internalRemoteQueues;
		private readonly IPublicRemoteQueue m_publicQueue;
		private readonly HealthCheckSettings m_settings;

		public bool IsHealthy => this.GetHealthStatus();

		public HealthMonitoringService(IInternalRemoteQueue @internal, IPublicRemoteQueue @public, IOptions<HealthCheckSettings> settings)
		{
			this.m_internalRemoteQueues = @internal;
			this.m_publicQueue = @public;
			this.m_settings = settings.Value;
		}

		public IEnumerable<string> GetHealthStatusExplanation()
		{
			var result = new List<string>();

			if(this.CheckLiveDataQueues()) {
				result.Add("Live Data Service queue out of bounds");
			}

			if(this.CheckTriggerQueues()) {
				result.Add("Trigger Service queue out of bounds");
			}

			if(this.CheckPublicMqttQueue()) {
				result.Add("Public MQTT queue is out of bounds");
			}

			return result;
		}

		private bool GetHealthStatus()
		{
			return this.CheckLiveDataQueues() && this.CheckTriggerQueues() && this.CheckPublicMqttQueue();
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
