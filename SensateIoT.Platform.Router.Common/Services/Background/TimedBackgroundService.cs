﻿/*
 * Background service, which is executed on a predefined interval.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SensateIoT.Platform.Router.Common.Services.Background
{
	public abstract class TimedBackgroundService : BackgroundService
	{
		private Timer _timer;

		private readonly TimeSpan m_startDelay;
		private readonly TimeSpan m_interval;

		protected TimedBackgroundService(TimeSpan startDelay, TimeSpan interval, ILogger logger) : base(logger)
		{
			this.m_startDelay = startDelay;
			this.m_interval = interval;
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			this._timer = new Timer(this.Invoke, null, this.m_startDelay, this.m_interval);

			return Task.CompletedTask;
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			await base.StopAsync(cancellationToken).ConfigureAwait(false);
			this._timer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		private async void Invoke(object arg)
		{
			await this.ExecuteAsync(this.m_stoppingCts.Token);
		}
	}
}
