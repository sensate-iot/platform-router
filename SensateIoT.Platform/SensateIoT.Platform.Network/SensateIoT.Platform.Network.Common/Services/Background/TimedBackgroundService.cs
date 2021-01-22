/*
 * Background service, which is executed on a predefined interval.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SensateIoT.Platform.Network.Common.Services.Background
{
	public abstract class TimedBackgroundService : BackgroundService
	{
		private Timer _timer;
		private long _millis;

		private readonly TimeSpan m_startDelay;
		private readonly TimeSpan m_interval;

		protected TimedBackgroundService(TimeSpan startDelay, TimeSpan interval)
		{
			this.m_startDelay = startDelay;
			this.m_interval = interval;
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			this._millis = 0L;
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
			Interlocked.Add(ref this._millis, Convert.ToInt64(this.m_interval.TotalMilliseconds));
			await this.ExecuteAsync(this.m_stoppingCts.Token);
		}
	}
}
