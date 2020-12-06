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
		private readonly IInternalRemoteQueue m_internalRemote;
		private readonly IRemoteStorageQueue m_remoteStorageQueue;

		public RoutingPublishService(IInternalRemoteQueue internalRemote,
									 IRemoteStorageQueue remoteStorage,
		                             IOptions<RoutingPublishSettings> options) : base(TimeSpan.FromSeconds(5), options.Value.InternalInterval)
		{
			this.m_internalRemote = internalRemote;
			this.m_remoteStorageQueue = remoteStorage;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			await Task.WhenAll(this.m_internalRemote.FlushAsync(),
			                   this.m_internalRemote.FlushLiveDataAsync(),
			                   this.m_remoteStorageQueue.FlushMessagesAsync()).ConfigureAwait(false);
		}
	}
}
