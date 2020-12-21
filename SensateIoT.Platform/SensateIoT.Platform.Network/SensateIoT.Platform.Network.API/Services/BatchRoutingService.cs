/*
 * Route messages in batches to the message router.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading.Tasks;
using System.Threading;

using SensateIoT.Platform.Network.API.Abstract;
using SensateIoT.Platform.Network.Common.Services.Background;

namespace SensateIoT.Platform.Network.API.Services
{
	public class BatchRoutingService : TimedBackgroundService
	{
		private readonly IMeasurementAuthorizationService m_service;

		public BatchRoutingService(IMeasurementAuthorizationService service) :
			base(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(500))
		{
			this.m_service = service;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			await this.m_service.ProcessAsync();
		}
	}
}
