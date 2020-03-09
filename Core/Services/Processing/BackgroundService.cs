/*
 * Abstract background service.
 * 
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace SensateService.Services.Processing
{
	public abstract class BackgroundService : IHostedService, IDisposable
	{
		private Task _executor;
		private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

		public abstract Task ExecuteAsync(CancellationToken token);

		public virtual Task StartAsync(CancellationToken cancellationToken)
		{
			this._executor = ExecuteAsync(this._stoppingCts.Token);

			if(this._executor.IsCompleted)
				return this._executor;

			return Task.CompletedTask;
		}

		public virtual async Task StopAsync(CancellationToken cancellationToken)
		{
			if(this._executor == null)
				return;

			try {
				this._stoppingCts.Cancel();
			} finally {
				await Task.WhenAny(this._executor, Task.Delay(Timeout.Infinite, cancellationToken));
			}
		}

		public void Dispose()
		{
			this._stoppingCts.Cancel();
		}
	}
}
