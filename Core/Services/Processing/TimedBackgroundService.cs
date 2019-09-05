/*
 * Background service based on System.Threading.Timer.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using SensateService.Services.Settings;

namespace SensateService.Services.Processing
{
	public abstract class TimedBackgroundService : IHostedService
	{
		private Timer _timer;
		private long _millis;
		private TimedBackgroundServiceSettings _settings;

		public virtual Task StartAsync(CancellationToken cancellationToken)
		{
			var settings = new TimedBackgroundServiceSettings();

			this.Configure(settings);
			this._settings = settings;
			this._millis = 0L;
			this._timer = new Timer(this.Invoke, null, settings.StartDelay, settings.Interval);

			return Task.CompletedTask;
		}

		public virtual Task StopAsync(CancellationToken cancellationToken)
		{
			this._timer.Change(Timeout.Infinite, Timeout.Infinite);
			return Task.CompletedTask;
		}

		public void Invoke(object arg)
		{
			Interlocked.Add(ref this._millis, this._settings.Interval);
			var task = Task.Run(async () => { await this.ProcessAsync(); });
			task.Wait();
		}

		public long MillisecondsElapsed()
		{
			return Interlocked.Add(ref this._millis, 0);
		}

		protected abstract Task ProcessAsync();
		protected abstract void Configure(TimedBackgroundServiceSettings settings);
	}
}