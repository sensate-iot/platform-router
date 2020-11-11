/*
 * Background service, which is executed on a predefined interval.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System.Threading.Tasks;

namespace SensateIoT.Platform.Network.Common.Services.Background
{
	public abstract class TimedBackgroundService : BackgroundService
	{
		private Timer _timer;
		private long _millis;

		private readonly int m_startDelay;
		private readonly int m_interval;

		protected TimedBackgroundService(int startDelay, int interval)
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

		public void Invoke(object arg)
		{
			Interlocked.Add(ref this._millis, this.m_interval);
			var task = Task.Run(async () => { await this.ExecuteAsync(this.m_stoppingCts.Token); });
			task.Wait();
		}

		public long MillisecondsElapsed()
		{
			return Interlocked.Add(ref this._millis, 0);
		}
	}
}
