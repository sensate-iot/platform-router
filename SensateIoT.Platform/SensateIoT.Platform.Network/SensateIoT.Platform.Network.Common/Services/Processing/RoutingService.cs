/*
 * Routing service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

using SensateIoT.Platform.Network.Common.Caching.Object;
using SensateIoT.Platform.Network.Common.Collections.Abstract;
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
		private readonly IMessageQueue m_messages;

		private const int DequeueCount = 1000;

		public RoutingService(IDataCache cache, IMessageQueue queue)
		{
			this.m_messages = queue;
			this.m_cache = cache;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			do {
				Sensor sensor = null;

				if(this.m_messages.Count <= 0) {
					Thread.Sleep(TimeSpan.FromMilliseconds(100));
					continue;
				}

				var messages = this.m_messages.DequeueRange(DequeueCount);
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
