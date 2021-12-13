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
		private readonly HealthCheckSettings m_settings;

		public bool IsHealthy => this.GetHealthStatus();

		public HealthMonitoringService(IInternalRemoteQueue @internal, IOptions<HealthCheckSettings> settings)
		{
			this.m_internalRemoteQueues = @internal;
			this.m_settings = settings.Value;
		}

		public IEnumerable<string> GetHealthStatusExplanation()
		{
			throw new System.NotImplementedException();
		}

		private bool GetHealthStatus()
		{
			return this.CheckLiveDataQueues() && this.CheckTriggerQueues();
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
