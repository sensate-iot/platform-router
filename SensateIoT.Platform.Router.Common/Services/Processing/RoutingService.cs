/*
 * Routing service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Exceptions;
using SensateIoT.Platform.Router.Common.Routing.Abstract;
using SensateIoT.Platform.Router.Common.Services.Background;
using SensateIoT.Platform.Router.Common.Settings;
using SensateIoT.Platform.Router.Data.Abstract;

namespace SensateIoT.Platform.Router.Common.Services.Processing
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

		private readonly ILogger<RoutingService> m_logger;
		private readonly IMessageRouter m_router;
		private readonly RoutingPublishSettings m_settings;

		public RoutingService(IMessageRouter router,
							  IOptions<RoutingPublishSettings> settings,
							  ILogger<RoutingService> logger) : base(logger)
		{
			this.m_settings = settings.Value;
			this.m_logger = logger;
			this.m_router = router;
		}

		protected override async Task ExecuteAsync(CancellationToken token)
		{
			do {
				var result = this.ExecuteRouter();
				await this.WaitForRouterIf(result, token).ConfigureAwait(false);
			} while(!token.IsCancellationRequested);
		}

		private bool ExecuteRouter()
		{
			try {
				return this.m_router.TryRoute();
			} catch(RouterException exception) {
				this.m_logger.LogError(exception, "Routing error!");
			}

			return true;
		}

		private async Task WaitForRouterIf(bool result, CancellationToken token)
		{
			if(result) {
				return;
			}

			try {
				await Task.Delay(this.m_settings.InternalInterval, token);
			} catch(OperationCanceledException) {
				this.m_logger.LogWarning("Routing task cancelled.");
			}
		}
	}
}
