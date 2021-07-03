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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SensateIoT.Platform.Network.Common.Collections.Abstract;
using SensateIoT.Platform.Network.Common.Routing.Abstract;
using SensateIoT.Platform.Network.Common.Services.Background;
using SensateIoT.Platform.Network.Common.Settings;
using SensateIoT.Platform.Network.Data.Abstract;

namespace SensateIoT.Platform.Network.Common.Services.Processing
{
	public class RoutingService : BackgroundService
	{
		/*
		 * Route messages through the platform:
		 *
		 *		1 Check validity;
		 *		2 Trigger routing;
		 *		3 Live data routing;
		 *		4 Forward to storage.
		 */

		private readonly IQueue<IPlatformMessage> m_messages;
		private readonly ILogger<RoutingService> m_logger;
		private readonly IMessageRouter m_router;
		private readonly RoutingPublishSettings m_settings;

		private const int DequeueCount = 1000;

		public RoutingService(IQueue<IPlatformMessage> queue,
							  IMessageRouter router,
							  IOptions<RoutingPublishSettings> settings,
							  ILogger<RoutingService> logger) : base(logger)
		{
			this.m_settings = settings.Value;
			this.m_messages = queue;
			this.m_logger = logger;
			this.m_router = router;
		}

		protected override async Task ExecuteAsync(CancellationToken token)
		{
			do {
				if(this.m_messages.Count <= 0) {
					try {
						await Task.Delay(this.m_settings.InternalInterval, token);
					} catch(OperationCanceledException) {
						this.m_logger.LogWarning("Routing task cancelled.");
					}

					continue;
				}

				var messages = this.m_messages.DequeueRange(DequeueCount).ToList();
				messages = messages.OrderBy(x => x.SensorID).ToList();
				this.m_logger.LogInformation("Routing {count} messages.", messages.Count);

				this.m_router.Route(messages);
			} while(!token.IsCancellationRequested);
		}
	}
}
