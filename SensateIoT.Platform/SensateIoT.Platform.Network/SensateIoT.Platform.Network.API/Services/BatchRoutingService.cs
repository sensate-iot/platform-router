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
		private readonly IMeasurementAuthorizationService m_measurements;
		private readonly IMessageAuthorizationService m_messages;

		public BatchRoutingService(IMeasurementAuthorizationService measurements, IMessageAuthorizationService messages) :
			base(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(500))
		{
			this.m_measurements = measurements;
			this.m_messages = messages;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			await Task.WhenAll(this.m_measurements.ProcessAsync(), this.m_messages.ProcessAsync()).ConfigureAwait(false);
		}
	}
}
