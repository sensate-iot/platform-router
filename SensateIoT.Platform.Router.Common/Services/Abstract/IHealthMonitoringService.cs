using System.Collections.Generic;

namespace SensateIoT.Platform.Router.Common.Services.Abstract
{
	public interface IHealthMonitoringService
	{
		bool IsHealthy { get; }
		IEnumerable<string> GetHealthStatusExplanation();
	}
}
