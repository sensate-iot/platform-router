/*
 * Routing service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Linq;
using System.Threading.Tasks;
using System.Threading;

using SensateIoT.Platform.Network.Common.Caching.Object;
using SensateIoT.Platform.Network.Common.Collections;
using SensateIoT.Platform.Network.Common.Services.Background;
using SensateIoT.Platform.Network.Data.Abstract;

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
		private readonly IQueue<IPlatformMessage> m_messages;

		public RoutingService(IDataCache cache, IQueue<IPlatformMessage> messages)
		{
			this.m_messages = messages;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			do {
				var messages = this.m_messages.ToArray();

				messages = messages.OrderBy(x => x.SensorID).ToArray();
			} while(!token.IsCancellationRequested);
		}
	}
}
