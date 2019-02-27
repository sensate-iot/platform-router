/*
 * Background service based on System.Threading.Timer.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
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

		public Task StartAsync(CancellationToken cancellationToken)
		{
			var settings = new TimedBackgroundServiceSettings();

			this.Configure(settings);
			this._timer = new Timer(this.Invoke, null, settings.StartDelay, settings.Interval);

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			this._timer.Change(Timeout.Infinite, Timeout.Infinite);
			return Task.CompletedTask;
		}

		public void Invoke(object arg)
		{
			var task = Task.Run(async () => { await this.ProcessAsync(); });
			task.Wait();
		}

		protected abstract Task ProcessAsync();
		protected abstract void Configure(TimedBackgroundServiceSettings settings);
	}
}