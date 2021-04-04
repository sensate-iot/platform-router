/*
 * Publish routed messages on output topics.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using SensateIoT.Platform.Network.Common.Collections.Abstract;
using SensateIoT.Platform.Network.Common.Collections.Remote;
using SensateIoT.Platform.Network.Common.Services.Background;
using SensateIoT.Platform.Network.Common.Settings;

namespace SensateIoT.Platform.Network.Common.Services.Processing
{
	public class ActuatorPublishService : TimedBackgroundService
	{
		private readonly IPublicRemoteQueue m_remote;

		public ActuatorPublishService(IPublicRemoteQueue remote,
									IOptions<RoutingPublishSettings> options) : base(TimeSpan.FromSeconds(5), options.Value.PublicInterval)
		{
			this.m_remote = remote;
		}

		protected override async Task ExecuteAsync(CancellationToken token)
		{
			await this.m_remote.FlushQueueAsync().ConfigureAwait(false);
		}
	}
}
