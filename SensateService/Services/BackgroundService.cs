/*
 * Base class for background services.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;

namespace SensateService.Services
{
	public abstract class BackgroundService : IHostedService
	{
		private Task _executor;
		private readonly CancellationTokenSource _cts;

		public BackgroundService()
		{
			this._cts = new CancellationTokenSource();
		}

		protected abstract Task ExecuteAsync(CancellationToken cancellationToken);

		public virtual Task StartAsync(CancellationToken cancellationToken)
		{
			this._executor = ExecuteAsync(this._cts.Token);

			if(this._executor.IsCompleted) {
				return _executor;
			}

			return Task.CompletedTask;
		}

		public virtual async Task StopAsync(CancellationToken cancellationToken)
		{
			if(this._executor == null)
				return;

			try {
				this._cts.Cancel();
			} finally {
				await Task.WhenAny(this._executor, Task.Delay(Timeout.Infinite,
										cancellationToken));
			}
		}
	}
}
