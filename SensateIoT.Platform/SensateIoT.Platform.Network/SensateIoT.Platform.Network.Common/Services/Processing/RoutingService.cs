/*
 * Routing service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using SensateIoT.Platform.Network.Common.Caching.Object;
using SensateIoT.Platform.Network.Common.Services.Background;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Common.Services.Processing
{
	public class RoutingService : BackgroundService, IRoutingService
	{
		/*
		 * Route messages through the platform:
		 *
		 *		1 Check validity;
		 *		2 Trigger routing;
		 *		3 Live data routing;
		 *		4 Forward to storage.
		 */

		private readonly IDataCache m_cache;
		private readonly IList<IPlatformMessage> m_messages;

		public RoutingService(IDataCache cache)
		{
			this.m_messages = new List<IPlatformMessage>();
			this.m_cache = cache;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			do {
				Sensor sensor = null;
				var messages = this.m_messages.ToArray();

				this.m_messages.Clear();
				messages = messages.OrderBy(x => x.SensorID).ToArray();

				foreach(var message in messages) {
					if(sensor?.ID != message.SensorID) {
						sensor = this.m_cache.GetSensor(message.SensorID);
					}

					if(sensor == null) {
						continue;
					}

					if(sensor.TriggerInformation.HasActions) {
						this.EnqueueToTriggerService(message, sensor.TriggerInformation.IsTextTrigger);
					}

					if(sensor.LiveDataRouting == null || sensor.LiveDataRouting?.Count <= 0) {
						continue;
					}

					foreach(var info in sensor.LiveDataRouting) {
						this.EnqueueTo(message, info);
					}
				}
			} while(!token.IsCancellationRequested);
		}

		private void EnqueueToTriggerService(IPlatformMessage message, bool isText)
		{

		}

		private void EnqueueTo(IPlatformMessage message, RoutingTarget target)
		{

		}
	}
}
