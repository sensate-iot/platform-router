/*
 * Routing service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System.Threading.Tasks;

using SensateIoT.Platform.Network.Common.Services.Background;

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
		public RoutingService()
		{

		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			await Task.CompletedTask;
		}
	}
}
