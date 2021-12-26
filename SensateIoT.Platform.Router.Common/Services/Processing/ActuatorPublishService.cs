/*
 * Publish routed messages on output topics.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Services.Background;
using SensateIoT.Platform.Router.Common.Settings;

namespace SensateIoT.Platform.Router.Common.Services.Processing
{
	public class ActuatorPublishService : TimedBackgroundService
	{
		private readonly IPublicRemoteQueue m_remote;

		public ActuatorPublishService(IPublicRemoteQueue remote,
									IOptions<RoutingQueueSettings> options,
									ILogger<ActuatorPublishService> logger) : base(TimeSpan.FromSeconds(5), options.Value.PublicInterval, logger)
		{
			this.m_remote = remote;
		}

		protected override async Task ExecuteAsync(CancellationToken token)
		{
			await this.m_remote.FlushQueueAsync().ConfigureAwait(false);
		}
	}
}
