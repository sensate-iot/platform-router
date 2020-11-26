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

using SensateIoT.Platform.Network.Common.Collections.Remote;
using SensateIoT.Platform.Network.Common.Services.Background;
using SensateIoT.Platform.Network.Common.Settings;

namespace SensateIoT.Platform.Network.Common.Services.Processing
{
	public class RoutingPublishService : TimedBackgroundService
	{
		private readonly IRemoteQueue m_remote;

		public RoutingPublishService(IRemoteQueue remote, IOptions<RoutingPublishSettings> options) : base(TimeSpan.FromSeconds(1), options.Value.Interval)
		{
			this.m_remote = remote;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			await Task.WhenAll(this.m_remote.FlushAsync(), this.m_remote.FlushLiveDataAsync()).ConfigureAwait(false);
		}
	}
}
