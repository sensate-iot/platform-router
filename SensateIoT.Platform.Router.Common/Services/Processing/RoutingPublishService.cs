﻿/*
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
	public class RoutingPublishService : TimedBackgroundService
	{
		private readonly IRemoteLiveDataQueue m_liveDataQueue;
		private readonly IRemoteTriggerQueue m_triggerQueue;
		private readonly IRemoteStorageQueue m_remoteStorageQueue;
		private readonly IRemoteNetworkEventQueue m_eventQueue;

		public RoutingPublishService(IRemoteLiveDataQueue liveDataQueue,
									 IRemoteTriggerQueue triggerQueue,
									 IRemoteStorageQueue remoteStorage,
									 IRemoteNetworkEventQueue remoteEvents,
									 IOptions<RoutingQueueSettings> options,
									 ILogger<RoutingPublishService> logger) : base(TimeSpan.FromSeconds(5), options.Value.InternalInterval, logger)
		{
			this.m_liveDataQueue = liveDataQueue;
			this.m_remoteStorageQueue = remoteStorage;
			this.m_eventQueue = remoteEvents;
			this.m_triggerQueue = triggerQueue;
		}

		protected override async Task ExecuteAsync(CancellationToken token)
		{
			await Task.WhenAll(this.m_liveDataQueue.FlushAsync(),
							   this.m_triggerQueue.FlushAsync(),
							   this.m_eventQueue.FlushEventsAsync(),
							   this.m_remoteStorageQueue.FlushMessagesAsync()).ConfigureAwait(false);
		}
	}
}
